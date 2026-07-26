import { Component, Input, Output, EventEmitter, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';

/**
 * File Uploader Component
 * Drag-and-drop file upload component
 * Usage: <app-file-uploader (filesSelected)="onFilesSelected($event)" />
 */
@Component({
  selector: 'app-file-uploader',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './file-uploader.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FileUploaderComponent {
  @Input() multiple = true;
  @Input() acceptedFormats = '.pdf,.doc,.docx,.xls,.xlsx,.jpg,.png';
  @Input() maxSize = 10; // MB

  @Output() filesSelected = new EventEmitter<File[]>();

  selectedFiles: File[] = [];
  isDragging = false;

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragging = true;
  }

  onDragLeave(): void {
    this.isDragging = false;
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragging = false;

    const files = event.dataTransfer?.files;
    if (files) {
      this.processFiles(files);
    }
  }

  onFileSelected(event: Event): void {
    const target = event.target as HTMLInputElement;
    const files = target.files;
    if (files) {
      this.processFiles(files);
    }
  }

  private processFiles(files: FileList): void {
    const fileArray = Array.from(files);

    if (!this.multiple) {
      this.selectedFiles = [fileArray[0]];
    } else {
      this.selectedFiles = [...this.selectedFiles, ...fileArray];
    }

    this.filesSelected.emit(this.selectedFiles);
  }

  removeFile(file: File): void {
    this.selectedFiles = this.selectedFiles.filter((f) => f !== file);
    this.filesSelected.emit(this.selectedFiles);
  }
}
