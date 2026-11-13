export type ChallengeStatus = 'Draft' | 'Published' | 'Retired' | 'Archived';

export interface DailyTimeWindowDto {
  start: string;
  end: string;
}

export interface LocationDto {
  latitude: number;
  longitude: number;
}

export interface SocialChallengeDto {
  id: string;
  name: string;
  status: ChallengeStatus;
  timeWindow: DailyTimeWindowDto;
  requiredPeople: number;
  rangeInMeters: number;
  xp: number;
  exitMessage: string;
  location: LocationDto | null;
}

export interface CreateSocialChallengeDto {
  name: string;
  timeWindow: DailyTimeWindowDto;
  requiredPeople: number;
  rangeInMeters: number;
  xp: number;
  exitMessage: string;
  location: LocationDto | null;
}

export interface UpdateSocialChallengeDto {
  name: string;
  timeWindow: DailyTimeWindowDto;
  requiredPeople: number;
  rangeInMeters: number;
  xp: number;
  exitMessage: string;
  location: LocationDto | null;
}
