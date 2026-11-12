import { Routes } from '@angular/router';
import { CreateBlog } from './blog-authoring/create-blog/create-blog';
import { MyBlogs } from './blog-authoring/my-blogs/my-blogs';
import { BlogDetail } from './blog-reading/blog-detail/blog-detail';
import { BlogList } from './blog-reading/blog-list/blog-list';
import { CreateClub } from './club-authoring/create-club/create-club';
import { MyClubs } from './club-authoring/my-clubs/my-clubs';
import { ClubDetail } from './club-reading/club-detail/club-detail';
import { ClubList } from './club-reading/club-list/club-list';

export const socialRoutes: Routes = [
  { path: '', component: BlogList },
  { path: 'mine', component: MyBlogs },
  { path: 'create', component: CreateBlog },
  { path: 'clubs', component: ClubList },
  { path: 'clubs/mine', component: MyClubs },
  { path: 'clubs/create', component: CreateClub },
  { path: 'clubs/:id', component: ClubDetail },
  { path: ':id', component: BlogDetail },
];
