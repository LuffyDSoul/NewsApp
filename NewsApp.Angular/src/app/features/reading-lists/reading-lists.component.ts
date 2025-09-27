import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-reading-lists',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="container">
      <div class="header">
        <h2>My Reading Lists</h2>
        <p>Organize your saved articles into custom reading lists.</p>
      </div>
      
      <div class="placeholder">
        <div class="placeholder-card">
          <h3>📚 Reading List Feature</h3>
          <p>This is where you'll be able to:</p>
          <ul>
            <li>Create custom reading lists</li>
            <li>Save articles to specific lists</li>
            <li>Mark articles as read/unread</li>
            <li>View reading statistics</li>
          </ul>
          <p><strong>Note:</strong> Make sure the backend API is running to use full functionality.</p>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .container {
      padding: 20px;
      max-width: 1200px;
      margin: 0 auto;
    }

    .header {
      text-align: center;
      margin-bottom: 40px;
    }

    .header h2 {
      color: #343a40;
      font-size: 2rem;
      margin-bottom: 10px;
    }

    .header p {
      color: #6c757d;
      font-size: 1.1rem;
    }

    .placeholder {
      display: flex;
      justify-content: center;
    }

    .placeholder-card {
      background: white;
      border: 2px dashed #dee2e6;
      border-radius: 12px;
      padding: 40px;
      text-align: center;
      max-width: 500px;
      box-shadow: 0 2px 10px rgba(0,0,0,0.1);
    }

    .placeholder-card h3 {
      color: #007bff;
      font-size: 1.5rem;
      margin-bottom: 20px;
    }

    .placeholder-card p {
      color: #6c757d;
      margin-bottom: 15px;
    }

    .placeholder-card ul {
      text-align: left;
      color: #495057;
      margin: 20px 0;
    }

    .placeholder-card li {
      margin-bottom: 8px;
    }
  `]
})
export class ReadingListsComponent {
  constructor() {}
}
