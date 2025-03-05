import { inject, Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { Member } from '../_models/member';

@Injectable({
  providedIn: 'root',
})
export class LikesService {
  private http = inject(HttpClient);
  baseUrl = environment.apiUrl;
  likeIds = signal<number[]>([]);

  constructor() {}

  toggleLike(targetId: number) {
    return this.http.post(`${this.baseUrl}likes/${targetId}`, {});
  }

  getLikes(predicate: string) {
    return this.http.get<Member[]>(
      `${this.baseUrl}likes?predicate=${predicate}`
    );
  }

  getLikeIds() {
    return this.http.get<number[]>(`${this.baseUrl}likes/list`).subscribe({
      next: (ids) => this.likeIds.set(ids),
    });
  }
}
