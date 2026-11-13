import { Routes } from '@angular/router';
import { GamesHome } from './games-home/games-home';
import { QuizList } from './quiz-authoring/quiz-list/quiz-list';
import { ChallengeList } from './social-challenge-authoring/challenge-list/challenge-list';
import { CreateChallenge } from './social-challenge-authoring/create-challenge/create-challenge';

export const gamesRoutes: Routes = [
  { path: '', component: GamesHome },
  { path: 'challenges', component: ChallengeList },
  { path: 'challenges/create', component: CreateChallenge },
  { path: 'quizzes', component: QuizList },
];
