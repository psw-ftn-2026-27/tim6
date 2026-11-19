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
  location: LocationDto | null;
}

export interface CreateQuizDto {
  name: string;
  instructions: string;
  xp: number;
  questions: QuestionDto[];
  location: LocationDto | null;
}

export interface UpdateQuizDto {
  name: string;
  instructions: string;
  xp: number;
  questions: QuestionDto[];
  location: LocationDto | null;
}

export interface QuestionForSolvingDto {
  text: string;
  options: string[];
}

export interface QuizForSolvingDto {
  id: string;
  name: string;
  instructions: string;
  questions: QuestionForSolvingDto[];
}

export interface SelectedAnswerDto {
  questionIndex: number;
  optionIndex: number;
}

export interface SubmitAttemptDto {
  position: LocationDto;
  answers: SelectedAnswerDto[];
}

export interface QuizAttemptResultDto {
  correctAnswers: number;
  questionCount: number;
  awardedXp: number;
  earnedXp: number;
  quizXp: number;
  solvedQuiz: boolean;
}

export interface HiddenPointDto {
  image: string;
  location: LocationDto;
}

export interface LocationChallengeDto {
  id: string;
  name: string;
  xp: number;
  status: ChallengeStatus;
  hiddenPoints: HiddenPointDto[];
  location: LocationDto | null;
}

export interface CreateLocationChallengeDto {
  name: string;
  xp: number;
  hiddenPoints: HiddenPointDto[];
  location: LocationDto | null;
}

export interface UpdateLocationChallengeDto {
  name: string;
  xp: number;
  hiddenPoints: HiddenPointDto[];
  location: LocationDto | null;
}

export type ChallengeKind = 'Quiz' | 'SocialChallenge';

export interface ChallengeOnMapDto {
  id: string;
  kind: ChallengeKind;
  name: string;
  xp: number;
  location: LocationDto;
  activationRadiusInMeters: number;
  isSolved: boolean;
  distanceInMeters: number | null;
  canActivate: boolean;
}
