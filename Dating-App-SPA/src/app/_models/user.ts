import { Photo } from './photo';
import { Template } from './template';

export interface User {
  id: number;
  username: string;
  knownAs: string;
  age: number;
  gender: string;
  created: Date;
  lastActive: any;
  photoUrl: string;
  city: string;
  country: string;
  interests?: string;
  introduction?: string;
  lookingFor?: string;
  photos?: Photo[];
  preferences?: Template;
  template?: Template;
}
