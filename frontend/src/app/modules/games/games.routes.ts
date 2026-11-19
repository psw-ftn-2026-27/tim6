import { Routes } from '@angular/router';
import { GamesHome } from './games-home/games-home';
import { LocationChallengeList } from './location-challenge-authoring/location-challenge-list/location-challenge-list';
import { AvailableChallenges } from './location-challenge-solving/available-challenges/available-challenges';
import { QuizList } from './quiz-authoring/quiz-list/quiz-list';
import { ChallengeList } from './social-challenge-authoring/challenge-list/challenge-list';
import { CreateChallenge } from './social-challenge-authoring/create-challenge/create-challenge';

export const gamesRoutes: Routes = [
  { path: '', component: GamesHome },
  { path: 'challenges', component: ChallengeList },
  { path: 'challenges/create', component: CreateChallenge },
  { path: 'quizzes', component: QuizList },
  { path: 'location-challenges', component: LocationChallengeList },
  { path: 'location-challenges/available', component: AvailableChallenges },
];
