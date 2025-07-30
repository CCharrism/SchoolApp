import { Component, OnInit, ChangeDetectorRef, AfterViewInit, ViewChild, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { AuthService } from '../../services/auth.service';
import { SchoolService } from '../../services/school.service';
import { User, CreateSchoolRequest, School } from '../../models/auth.models';
import { ManageSchoolsComponent } from '../manage-schools/manage-schools.component';
import * as L from 'leaflet';

// Fix for default markers in Leaflet
delete (L.Icon.Default.prototype as any)._getIconUrl;
L.Icon.Default.mergeOptions({
  iconRetinaUrl: '/assets/marker-icon-2x.png',
  iconUrl: '/assets/marker-icon.png',
  shadowUrl: '/assets/marker-shadow.png',
});

interface CityResult {
  display_name: string;
  lat: string;
  lon: string;
  place_id: string;
}

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule, HttpClientModule, ManageSchoolsComponent],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css'
})
export class DashboardComponent implements OnInit, AfterViewInit {
  @ViewChild('mapContainer', { static: false }) mapContainer!: ElementRef;
  currentUser: User | null = null;
  currentTime = new Date();
  activeMenu = 'dashboard';

  // Map related properties
  private map!: L.Map;
  private marker!: L.Marker;
  citySuggestions: CityResult[] = [];
  showSuggestions = false;
  mapInitialized = false;

  // School form data
  newSchool: CreateSchoolRequest = {
    schoolName: '',
    ownerName: '',
    ownerUsername: '',
    ownerPassword: '',
    address: '',
    phone: '',
    email: ''
  };

  schools: School[] = [];
  isLoading = false;
  errorMessage = '';
  successMessage = '';

  // Dashboard stats
  stats = {
    totalSchools: 12,
    activeSchools: 11,
    totalOwners: 12,
    pendingApprovals: 2
  };

  // Recent activities
  recentActivities = [
    { id: 1, type: 'school', message: 'New school "Greenwood Academy" was created', time: '2 hours ago', icon: 'fa-school' },
    { id: 2, type: 'owner', message: 'School owner John Smith logged in', time: '4 hours ago', icon: 'fa-user-tie' },
    { id: 3, type: 'system', message: 'Monthly reports generated successfully', time: '6 hours ago', icon: 'fa-chart-bar' },
    { id: 4, type: 'system', message: 'System backup completed successfully', time: '1 day ago', icon: 'fa-database' }
  ];

  // Quick stats for cards
  quickStats = [
    { title: 'Total Schools', value: '12', icon: 'fa-school', color: 'primary', change: '+2 new' },
    { title: 'Active Schools', value: '11', icon: 'fa-check-circle', color: 'success', change: '92%' },
    { title: 'School Owners', value: '12', icon: 'fa-user-tie', color: 'info', change: '+2 new' },
    { title: 'Pending Reviews', value: '2', icon: 'fa-clock', color: 'warning', change: '-1 today' }
  ];

  constructor(
    private authService: AuthService,
    private schoolService: SchoolService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.currentUser = this.authService.getCurrentUser();
    
    // Update time every second
    setInterval(() => {
      this.currentTime = new Date();
    }, 1000);

    // Load schools when component initializes
    this.loadSchools();
  }

  ngAfterViewInit(): void {
    // Initialize map when view is ready and add-school is active
    setTimeout(() => {
      if (this.activeMenu === 'add-school' && this.mapContainer) {
        this.initializeMap();
      }
    }, 500); // Increased delay for better stability
  }

  initializeMap(): void {
    if (this.mapInitialized || !this.mapContainer?.nativeElement) return;

    try {
      // Clear any existing map
      if (this.map) {
        this.map.remove();
      }

      // Create map with fixed center and zoom
      this.map = L.map(this.mapContainer.nativeElement, {
        center: [30.3753, 69.3451], // Pakistan center
        zoom: 5, // Zoomed to show Pakistan region
        zoomControl: true,
        scrollWheelZoom: true,
        doubleClickZoom: true,
        dragging: true
      });
      
      L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '© OpenStreetMap contributors',
        maxZoom: 18
      }).addTo(this.map);

      // Force map to invalidate size after container is visible
      setTimeout(() => {
        if (this.map) {
          this.map.invalidateSize();
        }
      }, 100);

      this.mapInitialized = true;
      console.log('Map initialized successfully');
    } catch (error) {
      console.error('Error initializing map:', error);
      this.mapInitialized = false;
    }
  }

  loadSchools(): void {
    this.schoolService.getAllSchools().subscribe({
      next: (schools) => {
        this.schools = schools;
        // Update stats based on actual data
        this.quickStats[0].value = schools.length.toString();
        this.quickStats[1].value = schools.filter(s => s.isActive).length.toString();
      },
      error: (error) => {
        console.error('Error loading schools:', error);
        // Keep using mock data for now
      }
    });
  }

  createSchool(): void {
    if (!this.isFormValid()) {
      this.errorMessage = 'Please fill in all required fields';
      this.cdr.detectChanges();
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';
    this.successMessage = '';
    this.cdr.detectChanges();

    this.schoolService.createSchool(this.newSchool).subscribe({
      next: (createdSchool) => {
        this.isLoading = false;
        this.successMessage = `School "${createdSchool.schoolName}" created successfully!`;
        this.resetForm();
        this.loadSchools(); // Reload the schools list
        
        // Add to recent activities
        this.recentActivities.unshift({
          id: Date.now(),
          type: 'school',
          message: `New school "${createdSchool.schoolName}" was created`,
          time: 'Just now',
          icon: 'fa-school'
        });
        this.cdr.detectChanges();
      },
      error: (error) => {
        this.isLoading = false;
        this.errorMessage = error.error?.message || 'Failed to create school. Please try again.';
        console.error('Error creating school:', error);
        this.cdr.detectChanges();
      }
    });
  }

  private isFormValid(): boolean {
    return !!(this.newSchool.schoolName && 
              this.newSchool.ownerName && 
              this.newSchool.ownerUsername && 
              this.newSchool.ownerPassword);
  }

  resetForm(): void {
    this.newSchool = {
      schoolName: '',
      ownerName: '',
      ownerUsername: '',
      ownerPassword: '',
      address: '',
      phone: '',
      email: ''
    };
  }

  clearMessages(): void {
    this.errorMessage = '';
    this.successMessage = '';
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  getGreeting(): string {
    const hour = new Date().getHours();
    if (hour < 12) return 'Good Morning';
    if (hour < 17) return 'Good Afternoon';
    return 'Good Evening';
  }

  getActivityIconClass(type: string): string {
    switch (type) {
      case 'school': return 'text-primary';
      case 'owner': return 'text-success';
      case 'system': return 'text-info';
      default: return 'text-muted';
    }
  }

  // Map and city search methods
  onAddressInput(event: any): void {
    const query = event.target.value;
    if (!query || query.length < 2) {
      this.citySuggestions = [];
      this.showSuggestions = false;
      return;
    }

    // Search for cities with Pakistan included and better filtering
    const searchUrl = `https://nominatim.openstreetmap.org/search?format=json&q=${encodeURIComponent(query)}&limit=10&countrycodes=pk,us,ca,gb,au,de,fr,it,es,nl,be,ch,at,se,no,dk,fi,in,bd,af,ir&addressdetails=1`;
    
    fetch(searchUrl)
      .then(res => res.json())
      .then((data: CityResult[]) => {
        // Better filtering for cities, towns, and villages
        this.citySuggestions = data.filter(item => {
          const displayName = item.display_name.toLowerCase();
          const hasCity = displayName.includes('city') || 
                         displayName.includes('town') || 
                         displayName.includes('village') ||
                         displayName.includes('district') ||
                         displayName.includes('tehsil') ||
                         displayName.includes('municipality');
          
          // For Pakistan, also include major areas and divisions
          const isPakistan = displayName.includes('pakistan');
          if (isPakistan) {
            return hasCity || 
                   displayName.includes('lahore') ||
                   displayName.includes('karachi') ||
                   displayName.includes('islamabad') ||
                   displayName.includes('rawalpindi') ||
                   displayName.includes('faisalabad') ||
                   displayName.includes('multan') ||
                   displayName.includes('peshawar') ||
                   displayName.includes('quetta') ||
                   displayName.includes('sialkot') ||
                   displayName.includes('gujranwala');
          }
          
          return hasCity;
        }).slice(0, 8);
        
        this.showSuggestions = this.citySuggestions.length > 0;
        this.cdr.detectChanges();
      })
      .catch(error => {
        console.error('Error fetching city suggestions:', error);
        this.showSuggestions = false;
      });
  }

  selectCity(city: CityResult): void {
    // Extract city name (first part before comma)
    const cityName = city.display_name.split(',')[0];
    this.newSchool.address = `${cityName}, ${city.display_name.split(',').slice(-1)[0]}`;
    this.showSuggestions = false;
    this.citySuggestions = [];

    // Initialize map if not already done
    if (!this.mapInitialized) {
      setTimeout(() => {
        this.initializeMap();
        setTimeout(() => {
          this.animateToCity(city);
        }, 300);
      }, 100);
    } else {
      this.animateToCity(city);
    }
  }

  private animateToCity(city: CityResult): void {
    if (!this.map) {
      console.error('Map not initialized');
      return;
    }

    try {
      const lat = parseFloat(city.lat);
      const lon = parseFloat(city.lon);

      if (isNaN(lat) || isNaN(lon)) {
        console.error('Invalid coordinates');
        return;
      }

      // Animate to the selected city with smooth transition
      this.map.flyTo([lat, lon], 12, {
        duration: 1.5, // Faster animation
        easeLinearity: 0.1
      });

      // Remove existing marker and add new one
      if (this.marker) {
        this.map.removeLayer(this.marker);
      }

      // Create custom marker icon with better visibility
      const customIcon = L.icon({
        iconUrl: '/assets/marker-icon.png',
        shadowUrl: '/assets/marker-shadow.png',
        iconSize: [25, 41],
        iconAnchor: [12, 41],
        popupAnchor: [1, -34],
        shadowSize: [41, 41]
      });

      // Add marker with popup
      this.marker = L.marker([lat, lon], { icon: customIcon })
        .addTo(this.map)
        .bindPopup(`<div style="text-align: center;">
          <strong style="color: #0d6efd;">${city.display_name.split(',')[0]}</strong><br>
          <small>${city.display_name}</small>
        </div>`)
        .openPopup();

    } catch (error) {
      console.error('Error animating to city:', error);
    }
  }

  onMenuChange(menu: string): void {
    this.activeMenu = menu;
    
    // Initialize map when switching to add-school
    if (menu === 'add-school') {
      // Reset map state
      this.mapInitialized = false;
      setTimeout(() => {
        this.initializeMap();
      }, 200);
    }
  }

  hideSuggestions(): void {
    // Hide suggestions after a small delay to allow click events
    setTimeout(() => {
      this.showSuggestions = false;
      this.cdr.detectChanges();
    }, 200);
  }
}
