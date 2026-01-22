import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-simple-reading-lists',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="container">
      <h2>Reading Lists</h2>
      <p>This is a simple reading lists page.</p>
      <p>The full functionality will be added once compilation issues are resolved.</p>
    </div>
  `,
  styles: [`
    .container {
      padding: 20px;
      max-width: 1200px;
      margin: 0 auto;
    }
    h2 {
      color: #343a40;
    }
  `]
})
export class SimpleReadingListsComponent {
}
