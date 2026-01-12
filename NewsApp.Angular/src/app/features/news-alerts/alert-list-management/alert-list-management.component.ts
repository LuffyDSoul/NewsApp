import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { NewsAlertService, NewsAlertListDto, CreateNewsAlertListDto, UpdateNewsAlertListDto } from '../../../core/services/news-alert.service';

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

  constructor(
    private newsAlertService: NewsAlertService,
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
      language: alert.languageCode
    };
    
    // Add keyword if present
    if (alert.keyword && alert.keyword.trim()) {
      queryParams.keyword = alert.keyword.trim();
    }
    
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
    if (!this.currentAlert.name || !this.currentAlert.keyword?.trim()) {
      alert('Please enter a name and at least one keyword');
      return;
    }

    // Set categories to empty or single default value to satisfy backend validation
    this.currentAlert.categories = 'general';

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
    return d.toLocaleString();
  }
}
