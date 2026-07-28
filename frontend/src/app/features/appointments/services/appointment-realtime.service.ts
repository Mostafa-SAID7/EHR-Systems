import { Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { Subject, Observable } from 'rxjs';
import { environment } from '@env/environment';

/**
 * Real-time appointment updates service using SignalR.
 * Handles WebSocket connections for live appointment notifications.
 */
@Injectable({
  providedIn: 'root'
})
export class AppointmentRealtimeService {
  private hubConnection: HubConnection | null = null;
  private appointmentUpdates$ = new Subject<any>();
  private connectionState$ = new Subject<boolean>();

  constructor() {}

  /**
   * Initialize SignalR connection.
   */
  public connect(): Promise<void> {
    if (this.hubConnection?.state === 1) {
      return Promise.resolve();
    }

    this.hubConnection = new HubConnectionBuilder()
      .withUrl(`${environment.apiUrl.replace('/api/v1', '')}/hubs/appointments`, {
        skipNegotiation: true,
        transport: 1 // WebSocket
      })
      .configureLogging(LogLevel.Information)
      .withAutomaticReconnect([0, 0, 0, 5000, 10000, 30000])
      .build();

    // Event handlers
    this.hubConnection.on('AppointmentScheduled', (appointment) => {
      this.appointmentUpdates$.next({
        type: 'scheduled',
        data: appointment
      });
    });

    this.hubConnection.on('AppointmentConfirmed', (appointmentId) => {
      this.appointmentUpdates$.next({
        type: 'confirmed',
        appointmentId
      });
    });

    this.hubConnection.on('AppointmentCancelled', (appointmentId, reason) => {
      this.appointmentUpdates$.next({
        type: 'cancelled',
        appointmentId,
        reason
      });
    });

    this.hubConnection.on('AppointmentStatusChanged', (appointmentId, newStatus) => {
      this.appointmentUpdates$.next({
        type: 'statusChanged',
        appointmentId,
        newStatus
      });
    });

    this.hubConnection.on('ReminderSent', (appointmentId, reminderType) => {
      this.appointmentUpdates$.next({
        type: 'reminderSent',
        appointmentId,
        reminderType
      });
    });

    this.hubConnection.on('NoteAdded', (appointmentId, note) => {
      this.appointmentUpdates$.next({
        type: 'noteAdded',
        appointmentId,
        note
      });
    });

    this.hubConnection.on('AppointmentRescheduled', (appointmentId, rescheduleInfo) => {
      this.appointmentUpdates$.next({
        type: 'rescheduled',
        appointmentId,
        rescheduleInfo
      });
    });

    this.hubConnection.on('UserJoined', (userInfo) => {
      this.appointmentUpdates$.next({
        type: 'userJoined',
        userInfo
      });
    });

    this.hubConnection.on('NotifyError', (message) => {
      this.appointmentUpdates$.next({
        type: 'error',
        message
      });
    });

    // Connection state handlers
    this.hubConnection.onreconnecting(() => {
      this.connectionState$.next(false);
    });

    this.hubConnection.onreconnected(() => {
      this.connectionState$.next(true);
    });

    return this.hubConnection.start()
      .then(() => {
        this.connectionState$.next(true);
        console.log('SignalR connected');
      })
      .catch(err => {
        this.connectionState$.next(false);
        console.error('SignalR connection failed:', err);
        throw err;
      });
  }

  /**
   * Disconnect from SignalR.
   */
  public async disconnect(): Promise<void> {
    if (this.hubConnection) {
      await this.hubConnection.stop();
      this.connectionState$.next(false);
    }
  }

  /**
   * Subscribe to a specific appointment for real-time updates.
   */
  public subscribeToAppointment(appointmentId: string): Promise<void> {
    if (!this.hubConnection) {
      throw new Error('SignalR not connected');
    }

    return this.hubConnection.invoke('SubscribeToAppointment', appointmentId)
      .catch(err => {
        console.error(`Failed to subscribe to appointment ${appointmentId}:`, err);
        throw err;
      });
  }

  /**
   * Unsubscribe from an appointment.
   */
  public unsubscribeFromAppointment(appointmentId: string): Promise<void> {
    if (!this.hubConnection) {
      return Promise.resolve();
    }

    return this.hubConnection.invoke('UnsubscribeFromAppointment', appointmentId)
      .catch(err => {
        console.error(`Failed to unsubscribe from appointment ${appointmentId}:`, err);
        throw err;
      });
  }

  /**
   * Get appointment updates observable.
   */
  public getUpdates(): Observable<any> {
    return this.appointmentUpdates$.asObservable();
  }

  /**
   * Get connection state observable.
   */
  public getConnectionState(): Observable<boolean> {
    return this.connectionState$.asObservable();
  }

  /**
   * Check if connected.
   */
  public isConnected(): boolean {
    return this.hubConnection?.state === 1;
  }
}
