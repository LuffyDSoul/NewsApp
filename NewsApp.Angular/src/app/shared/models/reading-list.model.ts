export interface ReadingListDto {
  id: string;
  name: string;
  description?: string;
  isPublic: boolean;
  color?: string;
  sortOrder: number;
  createdAt: Date;
  updatedAt?: Date;
  articleCount: number;
  unreadCount: number;
  ownerName?: string;
}

export interface CreateReadingListDto {
  name: string;
  description?: string;
  isPublic: boolean;
  color?: string;
  sortOrder: number;
}

export interface UpdateReadingListDto {
  name: string;
  description?: string;
  isPublic: boolean;
  color?: string;
  sortOrder: number;
}

export interface SavedArticleDto {
  id: string;
  readingListId?: string;
  readingListName?: string;
  source: string;
  title: string;
  description?: string;
  url: string;
  urlToImage?: string;
  publishedAt?: Date;
  content?: string;
  languageCode: string;
  author?: string;
  savedAt: Date;
  isRead: boolean;
  notes?: string;
  tags?: string;
}

export interface SaveArticleDto {
  readingListId?: string;
  source: string;
  title: string;
  description?: string;
  url: string;
  urlToImage?: string;
  publishedAt?: Date;
  content?: string;
  languageCode: string;
  author?: string;
  notes?: string;
  tags?: string;
}

export interface UpdateSavedArticleDto {
  readingListId?: string;
  isRead: boolean;
  notes?: string;
  tags?: string;
}

export interface BulkUpdateSavedArticlesDto {
  articleIds: string[];
  markAsRead?: boolean;
  moveToReadingListId?: string;
  addTags?: string;
  removeTags?: string;
}

export interface SavedArticleStatsDto {
  totalCount: number;
  unreadCount: number;
  savedThisWeekCount: number;
  readThisWeekCount: number;
}
