import { Component, OnDestroy, AfterViewInit } from '@angular/core';

@Component({
  selector: 'app-background',
  templateUrl: './background.html',
  styleUrls: ['./background.css']
})
export class YourComponent implements AfterViewInit, OnDestroy {
  private vantaEffect: any;

  ngAfterViewInit() {
    // Wait for VANTA and THREE to be available on window
    if (typeof (window as any).VANTA === 'undefined' || typeof (window as any).THREE === 'undefined') {
      // If not loaded, dynamically load them
      this.loadScript('https://cdnjs.cloudflare.com/ajax/libs/three.js/r134/three.min.js').then(() => {
        return this.loadScript('https://cdn.jsdelivr.net/npm/vanta@latest/dist/vanta.clouds.min.js');
      }).then(() => {
        this.initVanta();
      });
    } else {
      this.initVanta();
    }
  }

  initVanta() {
    this.vantaEffect = (window as any).VANTA.CLOUDS({
      el: '#vanta-clouds',
      mouseControls: true,
      touchControls: true,
      gyroControls: false,
      minHeight: 200.0,
      minWidth: 200.0,
      skyColor: 0xc9d5ed,
      cloudColor: 0x93ed97,
      sunColor: 0xb3aaa4,
      sunGlareColor: 0xfff6f3,
      sunlightColor: 0xd7a178,
      speed: 1.20
    });
  }

  loadScript(src: string): Promise<void> {
    return new Promise((resolve, reject) => {
      const script = document.createElement('script');
      script.src = src;
      script.onload = () => resolve();
      script.onerror = () => reject(`Failed to load script ${src}`);
      document.body.appendChild(script);
    });
  }

  ngOnDestroy() {
    if (this.vantaEffect) {
      this.vantaEffect.destroy();
    }
  }
}
