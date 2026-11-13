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

export interface AnswerOptionDto {
  text: string;
  isCorrect: boolean;
}

export interface QuestionDto {
  text: string;
  options: AnswerOptionDto[];
}

export interface QuizDto {
  id: string;
  name: string;
  instructions: string;
  xp: number;
  status: ChallengeStatus;
  questions: QuestionDto[];
}

export interface CreateQuizDto {
  name: string;
  instructions: string;
  xp: number;
  questions: QuestionDto[];
}

export interface UpdateQuizDto {
  name: string;
  instructions: string;
  xp: number;
  questions: QuestionDto[];
}
