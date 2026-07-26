import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-settings-page',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './settings-page.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SettingsPageComponent implements OnInit {
  activeSection = 'profile';
  saved = false;

  sections = [
    { key: 'profile',       label: 'Profile',       icon: 'M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z' },
    { key: 'notifications', label: 'Notifications',  icon: 'M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6.002 6.002 0 00-4-5.659V5a2 2 0 10-4 0v.341C7.67 6.165 6 8.388 6 11v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9' },
    { key: 'security',      label: 'Security',       icon: 'M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z' },
    { key: 'system',        label: 'System',         icon: 'M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z M15 12a3 3 0 11-6 0 3 3 0 016 0z' },
  ];

  profile = { firstName: 'Sarah', lastName: 'Admin', email: 'admin@ehrplatform.com', phone: '(555) 000-0000', title: 'System Administrator' };

  notifications = [
    { label: 'Critical Lab Results',   description: 'Notify when a critical lab value is flagged', email: true,  sms: true },
    { label: 'Appointment Reminders',  description: 'Send reminders 24 hours before appointments',  email: true,  sms: false },
    { label: 'New Patient Registration',description: 'Alert when a new patient is registered',       email: false, sms: false },
    { label: 'Prescription Approved',  description: 'Confirm when pharmacy processes a prescription',email: true,  sms: true },
    { label: 'System Alerts',          description: 'Security events and system maintenance notices', email: true,  sms: true },
  ];

  sessions = [
    { device: 'Chrome on Windows 11', location: 'New York, USA', time: 'Active now',   current: true },
    { device: 'Safari on iPhone 15',  location: 'New York, USA', time: '2 hours ago',  current: false },
    { device: 'Firefox on macOS',     location: 'Brooklyn, USA', time: '3 days ago',   current: false },
  ];

  toggles = [
    { label: 'Dark Mode (System Default)',    description: 'Match system appearance preference',       enabled: false },
    { label: 'Audit Trail Logging',          description: 'Log all user actions for compliance',      enabled: true },
    { label: 'Auto-Lock Screen',            description: 'Lock after inactivity timeout',            enabled: true },
    { label: 'Drug Interaction Alerts',     description: 'Show real-time drug interaction warnings', enabled: true },
  ];

  ngOnInit(): void {}
}
