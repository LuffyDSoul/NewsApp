import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

// Temporary simple interfaces for testing
export interface SimpleReadingListDto {
  id: string;
  name: string;
  description?: string;
  isPublic: boolean;
  articleCount: number;
  unreadCount: number;
  createdAt: Date;
}

export interface SimpleCreateReadingListDto {
  name: string;
  description?: string;
  isPublic: boolean;
  sortOrder: number;
}

@Injectable({
  providedIn: 'root'
})
export class SimpleReadingListService {
  private baseUrl = `${environment.apiUrl}/reading-lists`;

  constructor(private http: HttpClient) {}

  getMyReadingLists(): Observable<SimpleReadingListDto[]> {
    return this.http.get<SimpleReadingListDto[]>(`${this.baseUrl}/my-lists`);
  }

  createReadingList(input: SimpleCreateReadingListDto): Observable<SimpleReadingListDto> {
    return this.http.post<SimpleReadingListDto>(this.baseUrl, input);
  }
}
