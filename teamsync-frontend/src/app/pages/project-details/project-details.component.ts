import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-project-details',
  imports: [CommonModule],
  template: `
    <div class="project-details-container">
      <h1>Project Details</h1>
      <p>Project ID: {{ projectId }}</p>
    </div>
  `,
  styles: [
    `
      .project-details-container {
        padding: 24px;
      }

      h1 {
        font-size: 32px;
        font-weight: 700;
        color: #1a202c;
        margin-bottom: 16px;
      }
    `,
  ],
})
export class ProjectDetailsComponent {
  projectId: string | null = null;

  constructor(private route: ActivatedRoute) {
    this.projectId = this.route.snapshot.paramMap.get('id');
  }
}
