export type BlogStatus = 'Draft' | 'Published' | 'Closed';

export interface CommentDto {
  id: string;
  userId: string;
  text: string;
  createdAt: string;
}

export interface BlogDto {
  id: string;
  authorId: string;
  title: string;
  description: string;
  createdAt: string;
  lastModifiedAt: string;
  images: string[];
  status: BlogStatus;
  comments: CommentDto[];
}

export interface CreateBlogDto {
  title: string;
  description: string;
  images: string[] | null;
}

export interface UpdateBlogDto {
  title: string;
  description: string;
  images: string[] | null;
}

export interface CreateCommentDto {
  text: string;
}

export interface UpdateCommentDto {
  text: string;
}

export type BlogFollowState = 'Follow' | 'Pending' | 'Following' | 'OwnBlog';

export interface BlogFollowStatusDto {
  state: BlogFollowState;
}

export type FollowRequestStatus = 'Pending' | 'Accepted' | 'Rejected';

export interface SentFollowRequestDto {
  id: string;
  recipientId: string;
  createdAt: string;
  status: FollowRequestStatus;
}

export interface FollowedUserDto {
  userId: string;
}

export interface ReceivedFollowRequestDto {
  id: string;
  senderId: string;
  createdAt: string;
}

export interface FollowerDto {
  userId: string;
  isFollowedBack: boolean;
  hasPendingRequest: boolean;
}
  export interface ClubDto {
  id: string;
  ownerId: string;
  name: string;
  description: string;
  images: string[];
}

export interface CreateClubDto {
  name: string;
  description: string;
  images: string[];
}

export interface UpdateClubDto {
  name: string;
  description: string;
  images: string[];
}
