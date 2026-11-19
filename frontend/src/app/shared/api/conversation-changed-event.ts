export type ConversationChangeType =
  | 'MessageSent'
  | 'MessageDeleted'
  | 'ParticipantMuted'
  | 'ParticipantUnmuted';

export interface ConversationChangedEvent {
  conversationId: string;
  changeType: ConversationChangeType;
}
