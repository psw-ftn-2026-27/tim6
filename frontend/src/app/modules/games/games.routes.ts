import { Routes } from '@angular/router';
import { ChallengeMap } from './challenge-map/challenge-map/challenge-map';
import { GamesHome } from './games-home/games-home';
import { LocationChallengeList } from './location-challenge-authoring/location-challenge-list/location-challenge-list';
import { AvailableChallenges } from './location-challenge-solving/available-challenges/available-challenges';
import { QuizList } from './quiz-authoring/quiz-list/quiz-list';
import { SolveQuiz } from './quiz-solving/solve-quiz/solve-quiz';
import { ChallengeList } from './social-challenge-authoring/challenge-list/challenge-list';
import { CreateChallenge } from './social-challenge-authoring/create-challenge/create-challenge';
import { SolveSocialChallenge } from './social-challenge-solving/solve-social-challenge/solve-social-challenge';

export const gamesRoutes: Routes = [
  { path: '', component: GamesHome },
  { path: 'challenges', component: ChallengeList },
  { path: 'challenges/create', component: CreateChallenge },
  { path: 'challenge-map', component: ChallengeMap },
  { path: 'quizzes', component: QuizList },
  { path: 'quizzes/:id/solve', component: SolveQuiz },
  { path: 'location-challenges', component: LocationChallengeList },
  { path: 'social-challenges/:id/solve', component: SolveSocialChallenge },
  { path: 'location-challenges/available', component: AvailableChallenges },
];
