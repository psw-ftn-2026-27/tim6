import { Routes } from '@angular/router';
import { CreateBlog } from './blog-authoring/create-blog/create-blog';
import { MyBlogs } from './blog-authoring/my-blogs/my-blogs';
import { BlogDetail } from './blog-reading/blog-detail/blog-detail';
import { BlogList } from './blog-reading/blog-list/blog-list';
import { FollowedUsers } from './following/followed-users/followed-users';
import { Followers } from './following/followers/followers';
import { ReceivedFollowRequests } from './following/received-follow-requests/received-follow-requests';
import { SentFollowRequests } from './following/sent-follow-requests/sent-follow-requests';
import { CreateClub } from './club-authoring/create-club/create-club';
import { MyClubs } from './club-authoring/my-clubs/my-clubs';

export const socialRoutes: Routes = [
  { path: '', component: BlogList },
  { path: 'mine', component: MyBlogs },
  { path: 'create', component: CreateBlog },
  { path: 'following/requests', component: SentFollowRequests },
  { path: 'following', component: FollowedUsers },
  { path: 'followers/requests', component: ReceivedFollowRequests },
  { path: 'followers', component: Followers },
  { path: 'clubs/mine', component: MyClubs },
  { path: 'clubs/create', component: CreateClub },
  { path: ':id', component: BlogDetail },
];
