import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-test',
  standalone: true,
  template: `
    <div style="padding: 20px; text-align: center;">
      <h1>Test Component Works!</h1>
      <p>If you see this, Angular is working correctly.</p>
      <a routerLink="/login" style="color: blue; text-decoration: underline;">Go to Login</a>
    </div>
  `,
  imports: [RouterLink]
})
export class TestComponent {
}
