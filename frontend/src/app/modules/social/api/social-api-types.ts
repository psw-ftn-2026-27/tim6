export type BlogStatus = 'Draft' | 'Published' | 'Closed';

export interface CommentDto {
  id: string;
  userId: string;
  userEmail: string | null;
  text: string;
  createdAt: string;
  lastModifiedAt: string;
  editableUntil: string;
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
  score: number;
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

export type VoteSign = 'Up' | 'Down';

export interface CastVoteDto {
  sign: VoteSign;
}

export interface MyVoteDto {
  sign: VoteSign | null;
}

export type FollowStatus = 'Follow' | 'Pending' | 'Following';

export interface FollowStatusDto {
  status: FollowStatus;
}

export interface SentFollowRequestDto {
  id: string;
  recipientId: string;
  createdAt: string;
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
}
export type ClubStatus = 'Active' | 'Closed';

export interface ClubDto {
  id: string;
  ownerId: string;
  name: string;
  description: string;
  images: string[];
  status: ClubStatus;
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

export type ClubMembershipStatus = 'None' | 'Pending' | 'Member' | 'Closed';

export type ClubRole = 'Member' | 'Moderator' | 'Owner';

export interface ClubMembershipDto {
  status: ClubMembershipStatus;
  role: ClubRole | null;
}

export interface ClubJoinRequestDto {
  userId: string;
  userEmail: string | null;
  sentAt: string;
}

export interface ClubMemberDto {
  userId: string;
  userEmail: string | null;
  role: ClubRole;
}

export type ConversationRole = 'Participant' | 'Privileged';

export interface ConversationParticipantDto {
  userId: string;
  userEmail: string | null;
  role: ConversationRole;
  mutedUntil: string | null;
  isPermanentlyMuted: boolean;
}

export interface ConversationDto {
  id: string;
  contextType: string;
  contextId: string | null;
  participants: ConversationParticipantDto[];
  createdAt: string;
}

export interface ConversationSummaryDto {
  id: string;
  createdAt: string;
  participantIds: string[];
  otherParticipantEmail: string | null;
}

export interface ChatMessageDto {
  id: string;
  conversationId: string;
  authorId: string;
  content: string | null;
  createdAt: string;
  isDeleted: boolean;
  deletedAt: string | null;
}

export interface MessageCursorDto {
  createdAt: string;
  id: string;
}

export interface MessagePageDto {
  items: ChatMessageDto[];
  nextCursor: MessageCursorDto | null;
  hasMore: boolean;
}

export interface ConversationUserDto {
  userId: string;
  email: string | null;
}

export interface UserSearchPageDto {
  items: ConversationUserDto[];
  totalCount: number;
}

export interface SendMessageDto {
  content: string;
}

export interface ClubInvitationDto {
  userId: string;
  userEmail: string | null;
  sentAt: string;
}

export interface InviteDto {
  email: string;
}

export interface ReceivedClubInvitationDto {
  clubId: string;
  clubName: string;
  sentAt: string;
}
