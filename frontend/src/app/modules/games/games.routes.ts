import { Routes } from '@angular/router';
import { GamesHome } from './games-home/games-home';
import { QuizList } from './quiz-authoring/quiz-list/quiz-list';
import { PublishedQuizzes } from './quiz-solving/published-quizzes/published-quizzes';
import { SolveQuiz } from './quiz-solving/solve-quiz/solve-quiz';
import { ChallengeList } from './social-challenge-authoring/challenge-list/challenge-list';
import { CreateChallenge } from './social-challenge-authoring/create-challenge/create-challenge';

export const gamesRoutes: Routes = [
  { path: '', component: GamesHome },
  { path: 'challenges', component: ChallengeList },
  { path: 'challenges/create', component: CreateChallenge },
  { path: 'quizzes', component: QuizList },
  { path: 'quizzes/published', component: PublishedQuizzes },
  { path: 'quizzes/:id/solve', component: SolveQuiz },
];
