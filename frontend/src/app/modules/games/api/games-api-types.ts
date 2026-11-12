export type ChallengeStatus = 'Draft';

export interface DailyTimeWindowDto {
  start: string;
  end: string;
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
}

export interface CreateSocialChallengeDto {
  name: string;
  timeWindow: DailyTimeWindowDto;
  requiredPeople: number;
  rangeInMeters: number;
  xp: number;
  exitMessage: string;
}

export interface UpdateSocialChallengeDto {
  name: string;
  timeWindow: DailyTimeWindowDto;
  requiredPeople: number;
  rangeInMeters: number;
  xp: number;
  exitMessage: string;
}
