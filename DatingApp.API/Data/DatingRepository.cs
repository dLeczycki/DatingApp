using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data
{
    public class DatingRepository : IDatingRepository
    {
        private readonly DataContext _context;
        public DatingRepository(DataContext context)
        {
            _context = context;
        }
        public void Add<T>(T entity) where T : class
        {
            _context.Add(entity);
        }

        public void Delete<T>(T entity) where T : class
        {
            _context.Remove(entity);
        }

        public async Task<Like> GetLike(int userId, int recipientId)
        {
            return await _context.Likes.FirstOrDefaultAsync(u =>
                u.LikerId == userId && u.LikeeId == recipientId);
        }

        public async Task<Photo> GetPhoto(int id)
        {
            var photo = await _context.Photos.FirstOrDefaultAsync(p => p.Id == id);
            return photo;
        }
        public async Task<Photo> GetMainPhotoForUser(int userId)
        {
            return await _context.Photos.Where(u => u.UserId == userId).FirstOrDefaultAsync(p => p.IsMain);
        }

        public async Task<User> GetUser(int id)
        {
            var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id);

            return user;
        }

        public async Task<PagedList<User>> GetUsers(UserParams userParams)
        {
            var users = _context.Users.Include(u => u.Photos).Include(u => u.UsersTemplate).Include(u => u.Preferences)
            .OrderByDescending(u => u.LastActive)
            .AsQueryable();

            var preferences = await _context.Preferences.Where(p => p.UserId == userParams.UserId).FirstOrDefaultAsync();

            //Filters for get users
            users = users.Where(u => u.Id != userParams.UserId);

            users = users.Where(u => u.Gender == userParams.Gender);

            if (userParams.Likers)
            {
                var userLikers = await GetUserLikes(userParams.UserId, userParams.Likers);
                users = users.Where(u => userLikers.Contains(u.Id));
            }

            if (userParams.Likees)
            {
                var userLikees = await GetUserLikes(userParams.UserId, userParams.Likers);
                users = users.Where(u => userLikees.Contains(u.Id));
            }

            if (userParams.MinAge != 18 || userParams.MaxAge != 99)
            {
                var minDob = DateTime.Today.AddYears(-userParams.MaxAge - 1);
                var maxDob = DateTime.Today.AddYears(-userParams.MinAge);

                users = users.Where(u => u.DateOfBirth >= minDob && u.DateOfBirth <= maxDob);
            }

            if (!string.IsNullOrEmpty(userParams.OrderBy))
            {
                switch (userParams.OrderBy)
                {
                    case "created":
                        users = users.OrderByDescending(u => u.Created);
                        break;
                    case "preferences":
                        //Z tą pierwszą metodą jest coś nie tak
                        List<UserToSort> usersToSort = new List<UserToSort>();
                        UserToSort userToSort;
                        foreach (var user in users)
                        {
                            userToSort = new UserToSort
                            {
                                User = user,
                                Accuracy = CountAccuracy(user.UsersTemplate, preferences)
                            };
                            usersToSort.Add(userToSort);
                        }
                        usersToSort = usersToSort.OrderByDescending(u => u.Accuracy).ToList();
                        users = usersToSort.Select(u => u.User).AsQueryable();
                        break;
                    case "appearance":
                        List<UserToSort> usersToSortForAppearance = new List<UserToSort>();
                        UserToSort userToSortForAppearance;
                        foreach (var user in users)
                        {
                            userToSort = new UserToSort
                            {
                                User = user,
                                Accuracy = CountAccuracyForAppearance(user.UsersTemplate, preferences)
                            };
                            usersToSortForAppearance.Add(userToSort);
                        }
                        usersToSort = usersToSortForAppearance.OrderByDescending(u => u.Accuracy).ToList();
                        users = usersToSort.Select(u => u.User).AsQueryable();
                        break;
                    default:
                        users = users.OrderByDescending(u => u.LastActive);
                        break;
                }
            }

            return PagedList<User>.Create(users.ToList(), userParams.PageNumber, userParams.PageSize);
        }

        private async Task<IEnumerable<int>> GetUserLikes(int id, bool likers)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

            if (likers)
            {
                return user.Likers.Where(u => u.LikeeId == id).Select(i => i.LikerId);
            }
            else
            {
                return user.Likees.Where(u => u.LikerId == id).Select(i => i.LikeeId);
            }
        }

        public async Task<bool> SaveAll()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<Message> GetMessage(int id)
        {
            return await _context.Messages.FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<UsersTemplate> GetUsersTemplate(int userId)
        {
            return await _context.UsersTemplates.FirstOrDefaultAsync(t => t.UserId == userId);
        }

        public async Task<Preferences> GetUsersPreferences(int userId)
        {
            return await _context.Preferences.FirstOrDefaultAsync(p => p.UserId == userId);
        }

        public async Task<PagedList<Message>> GetMessagesForUser(MessageParams messageParams)
        {
            var messages = _context.Messages
                .AsQueryable();

            switch (messageParams.MessageContainer)
            {
                case "Inbox":
                    messages = messages.Where(u => u.RecipientId == messageParams.UserId && u.RecipientDeleted == false);
                    break;
                case "Outbox":
                    messages = messages.Where(u => u.SenderId == messageParams.UserId && u.SenderDeleted == false);
                    break;
                default:
                    messages = messages.Where(u => u.RecipientId == messageParams.UserId && u.RecipientDeleted == false && u.IsRead == false);
                    break;
            }

            messages = messages.OrderByDescending(d => d.MessageSent);
            return PagedList<Message>.Create(messages.ToList(), messageParams.PageNumber, messageParams.PageSize);
        }

        public async Task<IEnumerable<Message>> GetMessageThread(int userId, int recipientId)
        {
            var messages = await _context.Messages
                .Where(m => m.RecipientId == userId && m.RecipientDeleted == false && m.SenderId == recipientId
                    || m.RecipientId == recipientId && m.SenderDeleted == false && m.SenderId == userId)
                .OrderByDescending(m => m.MessageSent)
                .ToListAsync();

            return messages;
        }

        private double CountAccuracy(UsersTemplate template, Preferences preferences)
        {
            double accuracy = 0;
            if (template != null)
            {
                List<double> equasionValues = new List<double>();
                if (template.FacialHair == preferences.FacialHair) equasionValues.Add(1);
                else equasionValues.Add(0);
                if (template.Glasses == preferences.Glasses) equasionValues.Add(1);
                else equasionValues.Add(0);
                if (template.MakeUp == preferences.MakeUp) equasionValues.Add(1);
                else equasionValues.Add(0);
                if (template.Hair == preferences.Hair) equasionValues.Add(1);
                else equasionValues.Add(0);
                if (template.Personality == preferences.Personality) equasionValues.Add(1);
                else equasionValues.Add(0);
                if (template.Attitude == preferences.Attitude) equasionValues.Add(1);
                else equasionValues.Add(0);
                if (template.Assertive == preferences.Assertive) equasionValues.Add(1);
                else equasionValues.Add(0);
                if (template.Patriotic == preferences.Patriotic) equasionValues.Add(1);
                else equasionValues.Add(0);
                if (template.SelfConfident == preferences.SelfConfident) equasionValues.Add(1);
                else equasionValues.Add(0);
                if (template.WithSenseOfHumour == preferences.WithSenseOfHumour) equasionValues.Add(1);
                else equasionValues.Add(0);
                if (template.HardWorking == preferences.HardWorking) equasionValues.Add(1);
                else equasionValues.Add(0);
                if (template.Tolerant == preferences.Tolerant) equasionValues.Add(1);
                else equasionValues.Add(0);
                if (template.Kind == preferences.Kind) equasionValues.Add(1);
                else equasionValues.Add(0);
                accuracy = equasionValues.Average();
            }
            return accuracy;
        }

        private double CountAccuracyForAppearance(UsersTemplate template, Preferences preferences)
        {
            double accuracy = 0;
            if (template != null)
            {
                List<double> equasionValues = new List<double>();
                if (template.FacialHair == preferences.FacialHair) equasionValues.Add(1);
                else equasionValues.Add(0);
                if (template.Glasses == preferences.Glasses) equasionValues.Add(1);
                else equasionValues.Add(0);
                if (template.MakeUp == preferences.MakeUp) equasionValues.Add(1);
                else equasionValues.Add(0);
                if (template.Hair == preferences.Hair) equasionValues.Add(1);
                else equasionValues.Add(0);

                accuracy = equasionValues.Average();
            }

            return accuracy;
        }
    }
}