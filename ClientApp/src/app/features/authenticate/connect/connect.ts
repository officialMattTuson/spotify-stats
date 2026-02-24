import { Component } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-connect', 
  imports: [MatCardModule, MatButtonModule, MatIconModule],
  templateUrl: './connect.html',
  styleUrl: './connect.scss',
})
export class Connect {

  connectToSpotify() {
    
  }
}
