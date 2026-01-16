import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { NewsAlertService, NewsAlertNotificationDto } from '../../../core/services/news-alert.service';
import { interval, Subscription } from 'rxjs';
import { switchMap } from 'rxjs/operators';

@Component({
  selector: 'app-notifications-panel',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './notifications-panel.component.html',
  styleUrls: ['./notifications-panel.component.scss']
})
export class NotificationsPanelComponent implements OnInit, OnDestroy {
  notifications: NewsAlertNotificationDto[] = [];
  unreadCount = 0;
  isOpen = false;
  loading = false;
  showUnreadOnly = false;
  showToast = false;
  toastMessage = '';
  newNotifications: NewsAlertNotificationDto[] = [];

  private refreshSubscription?: Subscription;
  private previousUnreadCount = 0;
  private apiCallCount = 0;
  private readonly MAX_API_CALLS = 5;
  private notificationRefreshSubscription?: Subscription;

  constructor(
    private newsAlertService: NewsAlertService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadNotifications();
    this.loadUnreadCount();
    
    // Refresh unread count every 10 minutes and show toast for new notifications
    this.refreshSubscription = interval(600000)
      .pipe(switchMap(() => this.newsAlertService.getUnreadCount()))
      .subscribe(count => {
        // Check if there are new notifications
        if (count > this.previousUnreadCount && this.previousUnreadCount > 0) {
          const newCount = count - this.previousUnreadCount;
          this.showNewNotificationToast(newCount);
        }
        this.previousUnreadCount = count;
        this.unreadCount = count;
        
        if (this.isOpen) {
          this.loadNotifications();
        }
      });
    
    // Subscribe to manual refresh events
    this.notificationRefreshSubscription = this.newsAlertService.onNotificationsRefresh.subscribe(() => {
      console.log('Notification refresh triggered');
      this.loadNotifications();
      this.loadUnreadCount();
    });
  }

  ngOnDestroy(): void {
    this.refreshSubscription?.unsubscribe();
    this.notificationRefreshSubscription?.unsubscribe();
  }

  togglePanel(): void {
    this.isOpen = !this.isOpen;
    if (this.isOpen) {
      this.loadNotifications();
    }
  }

  closePanel(): void {
    this.isOpen = false;
  }

  goToManageAlerts(): void {
    this.closePanel();
    this.router.navigate(['/news-alerts']);
  }

  loadNotifications(): void {
    if (this.apiCallCount >= this.MAX_API_CALLS) {
      console.warn('Límite de llamadas API alcanzado');
      return;
    }
    
    this.loading = true;
    this.apiCallCount++;
    this.newsAlertService.getMyNotifications(this.showUnreadOnly).subscribe({
      next: (notifications) => {
        this.notifications = notifications;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading notifications:', error);
        this.loading = false;
      }
    });
  }

  loadUnreadCount(): void {
    if (this.apiCallCount >= this.MAX_API_CALLS) {
      console.warn('Límite de llamadas API alcanzado');
      return;
    }
    
    this.apiCallCount++;
    this.newsAlertService.getUnreadCount().subscribe({
      next: (count) => {
        this.unreadCount = count;
        this.previousUnreadCount = count; // Initialize previous count
      },
      error: (error) => {
        console.error('Error loading unread count:', error);
      }
    });
  }

  toggleUnreadFilter(): void {
    this.showUnreadOnly = !this.showUnreadOnly;
    this.loadNotifications();
  }

  markAsRead(notification: NewsAlertNotificationDto, event?: Event): void {
    if (event) {
      event.stopPropagation();
    }
    
    if (notification.isRead) return;
    
    if (this.apiCallCount >= this.MAX_API_CALLS) {
      console.warn('Límite de llamadas API alcanzado');
      return;
    }

    this.apiCallCount++;
    this.newsAlertService.markAsRead(notification.id).subscribe({
      next: () => {
        notification.isRead = true;
        this.loadUnreadCount();
      },
      error: (error) => {
        console.error('Error marking as read:', error);
      }
    });
  }

  markAllAsRead(): void {
    if (this.unreadCount === 0) return;
    
    if (this.apiCallCount >= this.MAX_API_CALLS) {
      console.warn('Límite de llamadas API alcanzado');
      return;
    }

    this.apiCallCount++;
    this.newsAlertService.markAllAsRead().subscribe({
      next: () => {
        // Remove unread notifications from the list if showing unread only
        if (this.showUnreadOnly) {
          this.notifications = [];
        } else {
          this.notifications.forEach(n => n.isRead = true);
        }
        this.unreadCount = 0;
      },
      error: (error) => {
        console.error('Error marking all as read:', error);
      }
    });
  }

  viewNews(notification: NewsAlertNotificationDto): void {
    // Mark as read if not already
    if (!notification.isRead) {
      this.markAsRead(notification);
    }

    // Navigate to news list to show this notification's articles
    this.closePanel();
    
    // First, get the alert configuration to know what to search for
    if (this.apiCallCount >= this.MAX_API_CALLS) {
      console.warn('Límite de llamadas API alcanzado');
      this.router.navigate(['/news']);
      return;
    }
    
    this.apiCallCount++;
    this.newsAlertService.getAlert(notification.newsAlertListId).subscribe({
      next: (alert) => {
        const queryParams: any = {
          alertName: alert.name,
          language: alert.languageCode,
          fromNotification: 'true',
          notificationId: notification.id,
          articleCount: notification.newArticlesCount
        };
        
        // Add keyword if present, otherwise add categories
        if (alert.keyword && alert.keyword.trim()) {
          queryParams.keyword = alert.keyword.trim();
        } else if (alert.categories && alert.categories.trim()) {
          queryParams.categories = alert.categories.trim();
        }
        
        this.router.navigate(['/alert-news'], { queryParams });
      },
      error: (error) => {
        console.error('Error loading alert:', error);
        // Fallback: navigate with basic info
        this.router.navigate(['/alert-news'], {
          queryParams: {
            category: notification.category,
            language: notification.languageCode,
            fromNotification: 'true',
            notificationId: notification.id,
            articleCount: notification.newArticlesCount
          }
        });
      }
    });
  }

  getTimeAgo(date: Date): string {
    const now = new Date();
    const notificationDate = new Date(date);
    const diffMs = now.getTime() - notificationDate.getTime();
    const diffMins = Math.floor(diffMs / 60000);
    const diffHours = Math.floor(diffMs / 3600000);
    const diffDays = Math.floor(diffMs / 86400000);

    if (diffMins < 1) return 'Just now';
    if (diffMins < 60) return `${diffMins} min ago`;
    if (diffHours < 24) return `${diffHours} hour${diffHours > 1 ? 's' : ''} ago`;
    if (diffDays < 7) return `${diffDays} day${diffDays > 1 ? 's' : ''} ago`;
    
    return notificationDate.toLocaleDateString();
  }

  getCategoryIcon(category: string): string {
    const icons: { [key: string]: string } = {
      'business': 'fa-briefcase',
      'entertainment': 'fa-film',
      'general': 'fa-newspaper',
      'health': 'fa-heartbeat',
      'science': 'fa-flask',
      'sports': 'fa-football-ball',
      'technology': 'fa-microchip'
    };
    return icons[category.toLowerCase()] || 'fa-newspaper';
  }

  private showNewNotificationToast(newCount: number): void {
    this.toastMessage = `${newCount} nueva${newCount > 1 ? 's' : ''} noticia${newCount > 1 ? 's' : ''} encontrada${newCount > 1 ? 's' : ''}`;
    this.showToast = true;
    
    // Auto-hide toast after 5 seconds
    setTimeout(() => {
      this.showToast = false;
    }, 5000);
  }
}
