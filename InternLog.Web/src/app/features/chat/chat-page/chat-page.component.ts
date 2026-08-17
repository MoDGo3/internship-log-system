import { Component, OnInit, ViewChild, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MessagesService } from '../../../core/services/messages.service';
import { AuthService } from '../../../core/auth/auth.service';
import { ConversationDto, MessageRecord } from '../../../models/message.model';
import { format } from 'date-fns';

@Component({
  selector: 'app-chat-page',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './chat-page.component.html',
  styleUrls: ['./chat-page.component.scss']
})
export class ChatPageComponent implements OnInit {
  conversations: ConversationDto[] = [];
  selectedConversation: ConversationDto | null = null;
  newMessage = '';
  messages: any[] = [];
  currentUserId: number = 0; // Or fetch from auth service

  @ViewChild('scrollMe') private myScrollContainer!: ElementRef;

  constructor(
    private messagesService: MessagesService,
    private authService: AuthService
  ) {}

  ngOnInit() {
    // In a real app, you get this from the auth token
    this.currentUserId = this.authService.currentUser()?.id || 0;
    this.loadConversations();
  }

  loadConversations() {
    this.messagesService.getConversations().subscribe({
      next: (res: any) => {
        const data = res.data;
        if (data) {
          this.conversations = data.map((c: any) => ({
            ...c,
            time: c.lastMessageAt ? format(new Date(c.lastMessageAt), 'HH:mm') : '',
            unread: c.unreadCount || 0
          }));
        }
      },
      error: (err: any) => console.error('Failed to load conversations', err)
    });
  }

  selectConversation(conv: ConversationDto) {
    this.selectedConversation = conv;
    conv.unreadCount = 0; // Mark as read in UI
    
    this.messagesService.getMessagesWithUser(conv.userID.toString()).subscribe({
      next: (res: any) => {
        const data = res.data;
        if (data) {
          this.messages = data.map((m: MessageRecord) => ({
            ...m,
            time: m.sentAt ? format(new Date(m.sentAt), 'HH:mm') : ''
          }));
          this.scrollToBottom();
          this.messagesService.markAsRead(conv.userID.toString()).subscribe();
        }
      },
      error: (err: any) => console.error('Failed to load messages', err)
    });
  }

  sendMessage() {
    if (!this.newMessage.trim() || !this.selectedConversation) return;
    
    const payload = {
      receiverId: this.selectedConversation.userID,
      content: this.newMessage
    };

    this.messagesService.sendMessage(payload).subscribe({
      next: (res: any) => {
        const sentMessage = res.data;
        this.messages.push({
          ...sentMessage,
          senderID: this.currentUserId,
          content: this.newMessage,
          time: format(new Date(), 'HH:mm')
        });
        
        this.newMessage = '';
        this.scrollToBottom();
      },
      error: (err: any) => console.error('Failed to send message', err)
    });
  }

  scrollToBottom(): void {
    setTimeout(() => {
      try {
        if (this.myScrollContainer) {
          this.myScrollContainer.nativeElement.scrollTop = this.myScrollContainer.nativeElement.scrollHeight;
        }
      } catch(err) { }
    }, 100);
  }
}
