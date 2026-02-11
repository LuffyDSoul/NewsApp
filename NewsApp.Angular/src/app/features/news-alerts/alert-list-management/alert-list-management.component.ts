import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { NewsAlertService, NewsAlertListDto, CreateNewsAlertListDto, UpdateNewsAlertListDto } from '../../../core/services/news-alert.service';
import { NotificationService } from '../../../core/services/notification.service';

@Component({
  selector: 'app-alert-list-management',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './alert-list-management.component.html',
  styleUrls: ['./alert-list-management.component.scss']
})
export class AlertListManagementComponent implements OnInit {
  alerts: NewsAlertListDto[] = [];
  loading = false;
  showModal = false;
  editMode = false;
  showDeleteModal = false;
  
  currentAlert: UpdateNewsAlertListDto = {
    name: '',
    description: '',
    categories: '',
    languageCode: 'en',
    keyword: '',
    isActive: true
  };
  
  editingAlertId: string | null = null;
  deletingAlertId: string | null = null;
  deletingAlertName: string = '';

  availableLanguages = [
    { code: 'de', name: 'German' },
    { code: 'en', name: 'English' },
    { code: 'es', name: 'Spanish' },
    { code: 'fr', name: 'French' },
    { code: 'he', name: 'Hebrew' },
    { code: 'it', name: 'Italian' },
    { code: 'nl', name: 'Dutch' },
    { code: 'no', name: 'Norwegian' },
    { code: 'pt', name: 'Portuguese' },
    { code: 'sv', name: 'Swedish' }
  ];

  alertNotificationCounts: { [alertId: string]: number } = {};
  
  // Email notifications
  sendingTestEmail = false;
  emailSuccessMessage = '';
  emailErrorMessage = '';

  constructor(
    private newsAlertService: NewsAlertService,
    private notificationService: NotificationService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadAlerts();
  }

  loadAlerts(): void {
    this.loading = true;
    this.newsAlertService.getMyAlerts().subscribe({
      next: (alerts) => {
        this.alerts = alerts;
        this.loading = false;
        
        // Load unread notification count for each alert
        alerts.forEach(alert => {
          this.loadAlertNotificationCount(alert.id);
        });
      },
      error: (error) => {
        console.error('Error loading alerts:', error);
        this.loading = false;
      }
    });
  }

  loadAlertNotificationCount(alertId: string): void {
    this.newsAlertService.getMyNotifications(true, 100).subscribe({
      next: (notifications) => {
        this.alertNotificationCounts[alertId] = notifications.filter(n => n.newsAlertListId === alertId).length;
      },
      error: (error) => {
        console.error('Error loading notification count:', error);
      }
    });
  }

  viewAlertNews(alert: NewsAlertListDto): void {
    // Navigate to alert-news page with alert filters
    const queryParams: any = {
      alertName: alert.name,
      keyword: alert.keyword || '',
      language: alert.languageCode
    };
    
    this.router.navigate(['/alert-news'], { queryParams });
  }

  openCreateModal(): void {
    this.editMode = false;
    this.currentAlert = {
      name: '',
      description: '',
      categories: 'general',
      languageCode: 'en',
      keyword: '',
      isActive: true
    };
    this.showModal = true;
  }

  openEditModal(alert: NewsAlertListDto): void {
    this.editMode = true;
    this.editingAlertId = alert.id;
    
    this.currentAlert = {
      name: alert.name,
      description: alert.description || '',
      categories: 'general',
      languageCode: alert.languageCode,
      keyword: alert.keyword || '',
      isActive: alert.isActive
    };
    
    this.showModal = true;
  }

  closeModal(): void {
    this.showModal = false;
    this.editingAlertId = null;
  }

  saveAlert(): void {
    if (!this.currentAlert.name) {
      alert('Please enter a name');
      return;
    }

    if (!this.currentAlert.keyword || !this.currentAlert.keyword.trim()) {
      alert('Keyword is required');
      return;
    }

    // Always set categories to 'general' as it's no longer used
    this.currentAlert.categories = 'general';
    this.currentAlert.keyword = this.currentAlert.keyword.trim();

    this.loading = true;

    if (this.editMode && this.editingAlertId) {
      // Update
      this.newsAlertService.updateAlert(this.editingAlertId, this.currentAlert).subscribe({
        next: () => {
          this.loadAlerts();
          this.closeModal();
        },
        error: (error) => {
          console.error('Error updating alert:', error);
          alert('Error updating alert: ' + (error.error?.error?.message || 'Unknown error'));
          this.loading = false;
        }
      });
    } else {
      // Create
      const createDto: CreateNewsAlertListDto = {
        ...this.currentAlert
      };
      this.newsAlertService.createAlert(createDto).subscribe({
        next: () => {
          this.loadAlerts();
          this.closeModal();
        },
        error: (error) => {
          console.error('Error creating alert:', error);
          alert('Error creating alert: ' + (error.error?.error?.message || 'Unknown error'));
          this.loading = false;
        }
      });
    }
  }

  openDeleteModal(alert: NewsAlertListDto): void {
    this.deletingAlertId = alert.id;
    this.deletingAlertName = alert.name;
    this.showDeleteModal = true;
  }

  closeDeleteModal(): void {
    this.showDeleteModal = false;
    this.deletingAlertId = null;
    this.deletingAlertName = '';
  }

  confirmDelete(): void {
    if (!this.deletingAlertId) return;

    this.loading = true;
    this.newsAlertService.deleteAlert(this.deletingAlertId).subscribe({
      next: () => {
        this.loadAlerts();
        this.closeDeleteModal();
      },
      error: (error) => {
        console.error('Error deleting alert:', error);
        alert('Error deleting alert');
        this.loading = false;
      }
    });
  }

  toggleAlertStatus(alert: NewsAlertListDto): void {
    const updateDto: UpdateNewsAlertListDto = {
      name: alert.name,
      description: alert.description || '',
      categories: alert.categories || 'general',
      languageCode: alert.languageCode,
      keyword: alert.keyword,
      isActive: !alert.isActive
    };

    this.newsAlertService.updateAlert(alert.id, updateDto).subscribe({
      next: () => {
        this.loadAlerts();
      },
      error: (error) => {
        console.error('Error toggling alert status:', error);
        window.alert('Error updating alert status');
      }
    });
  }

  getLanguageName(code: string): string {
    const lang = this.availableLanguages.find(l => l.code === code);
    return lang ? lang.name : code.toUpperCase();
  }

  formatDate(date?: Date | string): string {
    if (!date) return 'Never';
    const d = new Date(date);
    
    // Format: Jan 18, 2026 at 3:45 PM
    const options: Intl.DateTimeFormatOptions = {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: 'numeric',
      minute: '2-digit',
      hour12: true
    };
    
    return d.toLocaleString('en-US', options);
  }

  sendTestEmailNotification(): void {
    this.sendingTestEmail = true;
    this.emailSuccessMessage = '';
    this.emailErrorMessage = '';

    this.notificationService.sendTestNotification().subscribe({
      next: () => {
        this.sendingTestEmail = false;
        this.emailSuccessMessage = '✅ Test email sent successfully! Check your inbox (and spam folder).';
        
        // Clear message after 5 seconds
        setTimeout(() => {
          this.emailSuccessMessage = '';
        }, 5000);
      },
      error: (error) => {
        this.sendingTestEmail = false;
        this.emailErrorMessage = '❌ Error sending test email: ' + (error.error?.error?.message || 'Unknown error');
        console.error('Error sending test email:', error);
        
        // Clear message after 5 seconds
        setTimeout(() => {
          this.emailErrorMessage = '';
        }, 5000);
      }
    });
  }

  testAlert(alert: NewsAlertListDto): void {
    if (!alert.isActive) {
      window.alert('This alert is inactive. Please activate it first.');
      return;
    }

    const confirmed = window.confirm(`Test alert "${alert.name}"?\n\nThis will execute the alert now and send you an email if news articles are found.`);
    if (!confirmed) return;

    this.loading = true;
    this.emailSuccessMessage = '';
    this.emailErrorMessage = '';

    this.newsAlertService.testAlert(alert.id).subscribe({
      next: (result) => {
        this.loading = false;
        if (result.success) {
          this.emailSuccessMessage = `✅ ${result.message}${result.articlesFound ? ` (${result.articlesFound} articles found)` : ''}`;
          
          // Reload alerts to update last checked time
          this.loadAlerts();
          
          setTimeout(() => {
            this.emailSuccessMessage = '';
          }, 7000);
        } else {
          this.emailErrorMessage = `❌ ${result.message}`;
          setTimeout(() => {
            this.emailErrorMessage = '';
          }, 5000);
        }
      },
      error: (error) => {
        this.loading = false;
        this.emailErrorMessage = '❌ Error testing alert: ' + (error.error?.error?.message || 'Unknown error');
        console.error('Error testing alert:', error);
        
        setTimeout(() => {
          this.emailErrorMessage = '';
        }, 5000);
      }
    });
  }
}
