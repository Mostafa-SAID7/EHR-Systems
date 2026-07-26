import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { environment } from '@env/environment';
import { User } from '../../../core/models';

export interface UserListResponse {
  items: User[];
  total: number;
  pageNumber: number;
  pageSize: number;
}

export interface CreateUserRequest {
  email: string;
  firstName: string;
  lastName: string;
  role: string;
  department?: string;
}

export interface UpdateUserRequest {
  userId?: string;
  firstName: string;
  lastName: string;
  department?: string;
  isActive: boolean;
}

/**
 * User Management Service
 * Communicates with backend Identity Users API (/api/v1/users)
 */
@Injectable({ providedIn: 'root' })
export class UserService {
  private readonly apiUrl = `${environment.apiUrl}/users`;

  constructor(private http: HttpClient) {}

  /**
   * Fetch paginated list of users
   */
  getUsers(pageNumber = 1, pageSize = 20, search = ''): Observable<UserListResponse> {
    let params = new HttpParams()
      .set('pageNumber', pageNumber)
      .set('pageSize', pageSize);

    if (search) {
      params = params.set('search', search);
    }

    return this.http.get<UserListResponse>(this.apiUrl, { params });
  }

  /**
   * Fetch single user details by ID
   */
  getUserById(id: string): Observable<User> {
    return this.http.get<User>(`${this.apiUrl}/${id}`);
  }

  /**
   * Create a new user
   */
  createUser(req: CreateUserRequest): Observable<any> {
    return this.http.post<any>(this.apiUrl, req);
  }

  /**
   * Update existing user details
   */
  updateUser(id: string, req: UpdateUserRequest): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/${id}`, req);
  }

  /**
   * Deactivate user account
   */
  deleteUser(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  /**
   * Unlock user account
   */
  unlockUser(id: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${id}/unlock`, {});
  }
}
