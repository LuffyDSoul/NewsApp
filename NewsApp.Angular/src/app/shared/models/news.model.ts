export interface NewsArticleDto {
  id?: string;
  source: string;
  title: string;
  description: string;
  url: string;
  urlToImage?: string;
  publishedAt: Date;
  content?: string;
  author?: string;
  languageCode: string;
  creationTime?: Date;
}

export interface NewsSearchDto {
  query: string;
  category?: string;
  language?: string;
  pageSize?: number;
}

export interface PagedResultDto<T> {
  totalCount: number;
  items: T[];
}

export interface NewsSourceDto {
  id: string;
  name: string;
  description: string;
  url: string;
  category: string;
  language: string;
  country: string;
}

export interface CreateNewsArticleDto {
  source: string;
  title: string;
  description: string;
  url: string;
  urlToImage?: string;
  publishedAt: Date;
  content?: string;
  author?: string;
  languageCode: string;
}
