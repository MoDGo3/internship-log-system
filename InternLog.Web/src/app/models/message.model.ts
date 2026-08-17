export interface MessageRecord {
  messageID: number;
  senderID: number;
  receiverID: number;
  content: string;
  sentAt: string;
  readAt: string | null;
}

export interface ConversationDto {
  userID: number;
  name: string;
  avatar: string;
  lastMessage: string;
  lastMessageAt: string;
  unreadCount: number;
  isOnline: boolean;
}
