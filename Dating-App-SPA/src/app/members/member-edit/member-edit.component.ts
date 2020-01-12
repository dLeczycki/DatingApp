import { Component, OnInit, ViewChild, HostListener } from '@angular/core';
import { User } from 'src/app/_models/user';
import { ActivatedRoute } from '@angular/router';
import { AlertifyService } from 'src/app/_services/alertify.service';
import { NgForm } from '@angular/forms';
import { UserService } from 'src/app/_services/user.service';
import { AuthService } from 'src/app/_services/auth.service';

@Component({
  selector: 'app-member-edit',
  templateUrl: './member-edit.component.html',
  styleUrls: ['./member-edit.component.css']
})
export class MemberEditComponent implements OnInit {
  @ViewChild('editForm', { static: true }) editForm: NgForm;
  @ViewChild('preferencesForm', { static: true }) preferencesForm: NgForm;
  @ViewChild('templateForm', { static: true }) templateForm: NgForm;
  user: User;
  preferences: any;
  template: any;
  photoUrl: string;
  @HostListener('window:beforeunload', ['$event'])
  unloadNotification($event: any) {
    if (this.editForm.dirty) {
      $event.returnValue = true;
    }
  }

  constructor(
    private route: ActivatedRoute,
    private alertify: AlertifyService,
    private userService: UserService,
    private authService: AuthService
  ) { }

  ngOnInit() {
    this.route.data.subscribe(data => {
      this.user = data['user'];
      this.preferences = data['user'].preferences;
      this.template = data['user'].usersTemplate;
      this.convertPreferencesToString();
      this.convertTemplateToString();
    });
    this.authService.currentPhotoUr.subscribe(photoUrl => this.photoUrl = photoUrl);
  }


  updateUser() {
    this.userService
      .updateUser(this.authService.decodedToken.nameid, this.user)
      .subscribe(
        next => {
          this.alertify.success('Profil zaktualizowany pomyślnie');
          this.editForm.reset(this.user);
        },
        error => {
          this.alertify.error(error);
        }
      );
  }

  updatePreferences() {
    this.convertPreferencesToModel();
    this.userService.setPreferences(this.authService.decodedToken.nameid, this.preferences).subscribe(
      next => {
        this.alertify.success('Preferencje zaktualizowane pomyślnie');
        this.convertPreferencesToString();
        this.preferencesForm.reset(this.preferences);
      },
      error => {
        this.alertify.error(error);
      }
    );
  }

  updateTemplate() {
    this.convertTemplateToModel();
    this.userService.setTemplate(this.authService.decodedToken.nameid, this.template).subscribe(
      next => {
        this.alertify.success('Cechy charakteru zaktualizowane pomyślnie');
        this.convertTemplateToString();
        this.templateForm.reset(this.template);
      },
      error => {
        this.alertify.error(error);
      }
    );
  }

  convertPreferencesToModel() {
    this.preferences.assertive = JSON.parse(this.preferences.assertive);
    this.preferences.patriotic = JSON.parse(this.preferences.patriotic);
    this.preferences.selfConfident = JSON.parse(this.preferences.selfConfident);
    this.preferences.withSenseOfHumour = JSON.parse(this.preferences.withSenseOfHumour);
    this.preferences.hardWorking = JSON.parse(this.preferences.hardWorking);
    this.preferences.tolerant = JSON.parse(this.preferences.tolerant);
    this.preferences.kind = JSON.parse(this.preferences.kind);
    this.preferences.makeUp = JSON.parse(this.preferences.makeUp);
    this.preferences.facialHair = +this.preferences.facialHair;
  }

  convertPreferencesToString() {
    this.preferences.assertive = String(this.preferences.assertive);
    this.preferences.patriotic = String(this.preferences.patriotic);
    this.preferences.selfConfident = String(this.preferences.selfConfident);
    this.preferences.withSenseOfHumour = String(this.preferences.withSenseOfHumour);
    this.preferences.hardWorking = String(this.preferences.hardWorking);
    this.preferences.tolerant = String(this.preferences.tolerant);
    this.preferences.kind = String(this.preferences.kind);
    this.preferences.makeUp = String(this.preferences.makeUp);
    this.preferences.facialHair = String(this.preferences.facialHair);
  }

  convertTemplateToModel() {
    this.template.assertive = JSON.parse(this.preferences.assertive);
    this.template.patriotic = JSON.parse(this.preferences.patriotic);
    this.template.selfConfident = JSON.parse(this.preferences.selfConfident);
    this.template.withSenseOfHumour = JSON.parse(this.preferences.withSenseOfHumour);
    this.template.hardWorking = JSON.parse(this.preferences.hardWorking);
    this.template.tolerant = JSON.parse(this.preferences.tolerant);
    this.template.kind = JSON.parse(this.preferences.kind);
  }

  convertTemplateToString() {
    this.template.assertive = String(this.template.assertive);
    this.template.patriotic = String(this.template.patriotic);
    this.template.selfConfident = String(this.template.selfConfident);
    this.template.withSenseOfHumour = String(this.template.withSenseOfHumour);
    this.template.hardWorking = String(this.template.hardWorking);
    this.template.tolerant = String(this.template.tolerant);
    this.template.kind = String(this.template.kind);
  }

  updateMainPhoto(photoUrl) {
    this.user.photoUrl = photoUrl;
  }
}
