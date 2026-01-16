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

  availableCategories = [
    { value: 'business', name: 'Business' },
    { value: 'entertainment', name: 'Entertainment' },
    { value: 'general', name: 'General' },
    { value: 'health', name: 'Health' },
    { value: 'science', name: 'Science' },
    { value: 'sports', name: 'Sports' },
    { value: 'technology', name: 'Technology' }
  ];

  selectedCategories: Set<string> = new Set<string>();

  alertNotificationCounts: { [alertId: string]: number } = {};

  // API call limits - 5 per category checked
  private apiCallCounts: Map<string, number> = new Map();
  private readonly MAX_API_CALLS_PER_CATEGORY = 5;

  constructor(
    private newsAlertService: NewsAlertService,
    private router: Router
  ) {}

  isCategorySelected(category: string): boolean {
    return this.selectedCategories.has(category);
  }

  toggleCategory(category: string, event: Event): void {
    const checkbox = event.target as HTMLInputElement;
    const categoryKey = `category_${category}`;
    
    if (checkbox.checked) {
      // Initialize API call count for this category
      if (!this.apiCallCounts.has(categoryKey)) {
        this.apiCallCounts.set(categoryKey, 0);
      }
      this.selectedCategories.add(category);
    } else {
      this.selectedCategories.delete(category);
      // Remove API call count for this category
      this.apiCallCounts.delete(categoryKey);
    }
    // Update the categories string
    this.currentAlert.categories = Array.from(this.selectedCategories).join(',');
  }

  areCategoriesDisabled(): boolean {
    return !!(this.currentAlert.keyword && this.currentAlert.keyword.trim());
  }

  isKeywordDisabled(): boolean {
    return this.selectedCategories.size > 0;
  }

  onKeywordInput(): void {
    // If user starts typing keywords, clear selected categories
    if (this.currentAlert.keyword && this.currentAlert.keyword.trim()) {
      this.selectedCategories.clear();
      this.currentAlert.categories = '';
    }
  }

  checkAlertsNow(): void {
    // Check API call limits for each category
    for (const category of this.selectedCategories) {
      const categoryKey = `category_${category}`;
      const currentCount = this.apiCallCounts.get(categoryKey) || 0;
      
      if (currentCount >= this.MAX_API_CALLS_PER_CATEGORY) {
        alert(`Se ha alcanzado el límite de ${this.MAX_API_CALLS_PER_CATEGORY} llamadas para la categoría: ${category}`);
        return;
      }
    }
    
    // Also check keyword limit if keyword is set
    if (this.currentAlert.keyword && this.currentAlert.keyword.trim()) {
      const keywordKey = `keyword_${this.currentAlert.keyword}`;
      const currentCount = this.apiCallCounts.get(keywordKey) || 0;
      
      if (currentCount >= this.MAX_API_CALLS_PER_CATEGORY) {
        alert(`Se ha alcanzado el límite de ${this.MAX_API_CALLS_PER_CATEGORY} llamadas para el keyword: ${this.currentAlert.keyword}`);
        return;
      }
      
      // Increment keyword call count
      this.apiCallCounts.set(keywordKey, currentCount + 1);
    } else {
      // Increment category call counts
      for (const category of this.selectedCategories) {
        const categoryKey = `category_${category}`;
        const currentCount = this.apiCallCounts.get(categoryKey) || 0;
        this.apiCallCounts.set(categoryKey, currentCount + 1);
      }
    }
    
    this.loading = true;
    this.newsAlertService.triggerManualCheck().subscribe({
      next: (response) => {
        this.loading = false;
        // Reload alerts and notification counts after manual check
        this.loadAlerts();
        
        // Trigger refresh of notification panel
        this.newsAlertService.refreshNotifications();
        
        // Show success message
        const message = response.message || 'Alert check completed successfully!';
        alert(`Alert check completed!\n\n${message}\n\nPlease check your notifications panel for new alerts.`);
      },
      error: (error) => {
        console.error('Error checking alerts:', error);
        alert('Error checking alerts: ' + (error.error?.error?.message || 'Unknown error'));
        this.loading = false;
      }
    });
  }

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
    
    // Add keyword if present, otherwise add categories
    if (alert.keyword && alert.keyword.trim()) {
      queryParams.keyword = alert.keyword.trim();
    } else if (alert.categories && alert.categories.trim() && alert.categories !== 'general') {
      // Send categories (plural) for multiple category support
      queryParams.categories = alert.categories.trim();
    }
    
    this.router.navigate(['/alert-news'], { queryParams });
  }

  openCreateModal(): void {
    this.editMode = false;
    this.selectedCategories.clear();
    this.currentAlert = {
      name: '',
      description: '',
      categories: '',
      languageCode: 'en',
      keyword: '',
      isActive: true
    };
    this.showModal = true;
  }

  openEditModal(alert: NewsAlertListDto): void {
    this.editMode = true;
    this.editingAlertId = alert.id;
    
    // Clear previous state
    this.selectedCategories.clear();
    
    // Determine if this alert uses keywords or categories
    const hasKeyword = alert.keyword && alert.keyword.trim();
    const hasCategories = alert.categories && alert.categories.trim();
    
    // If alert has keywords, only load keywords (ignore categories)
    if (hasKeyword) {
      this.currentAlert = {
        name: alert.name,
        description: alert.description || '',
        categories: '',  // Clear categories
        languageCode: alert.languageCode,
        keyword: alert.keyword || '',
        isActive: alert.isActive
      };
      // Don't load any categories into selectedCategories
    } else if (hasCategories) {
      // Alert uses categories, load them
      const cats = alert.categories.split(',').map(c => c.trim()).filter(c => c);
      cats.forEach(cat => this.selectedCategories.add(cat));
      
      this.currentAlert = {
        name: alert.name,
        description: alert.description || '',
        categories: alert.categories || '',
        languageCode: alert.languageCode,
        keyword: '',  // Clear keyword
        isActive: alert.isActive
      };
    } else {
      // No keyword or categories (shouldn't happen, but handle it)
      this.currentAlert = {
        name: alert.name,
        description: alert.description || '',
        categories: '',
        languageCode: alert.languageCode,
        keyword: '',
        isActive: alert.isActive
      };
    }
    
    this.showModal = true;
  }

  closeModal(): void {
    this.showModal = false;
    this.editingAlertId = null;
    this.selectedCategories.clear();
  }

  saveAlert(): void {
    if (!this.currentAlert.name) {
      alert('Please enter a name');
      return;
    }

    // Update categories from selectedCategories set
    this.currentAlert.categories = Array.from(this.selectedCategories).join(',');

    // Validar que se haya seleccionado categoría O keyword, pero no ambas
    const hasCategory = this.currentAlert.categories && this.currentAlert.categories.trim();
    const hasKeyword = this.currentAlert.keyword && this.currentAlert.keyword.trim();

    if (!hasCategory && !hasKeyword) {
      alert('Please select a category OR enter keywords (at least one is required)');
      return;
    }

    if (hasCategory && hasKeyword) {
      alert('Please select either a category OR keywords, not both');
      return;
    }

    // Clear the field that's not being used
    if (hasCategory) {
      this.currentAlert.keyword = '';
    } else if (hasKeyword) {
      this.currentAlert.categories = '';
    }

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
