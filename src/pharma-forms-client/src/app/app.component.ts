import { Component, OnInit } from '@angular/core';
import { Router, NavigationEnd } from '@angular/router';
import { TranslateService } from '@ngx-translate/core';
import { filter } from 'rxjs/operators';
import { Title } from '@angular/platform-browser';
import { environment } from '../environments/environment';
import { AuthService } from './core/services/auth.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit {
  title = 'Pharmaceutical Forms System';
  isAuthenticated = false;
  isRtl = true;

  constructor(
    private router: Router,
    private titleService: Title,
    private translate: TranslateService,
    private authService: AuthService
  ) {
    // Set default language (Arabic)
    this.translate.setDefaultLang(environment.defaultLanguage);
    this.translate.use(environment.defaultLanguage);
    
    // Set page direction based on language
    this.isRtl = environment.defaultLanguage.startsWith('ar');
    document.documentElement.dir = this.isRtl ? 'rtl' : 'ltr';
    document.documentElement.lang = environment.defaultLanguage;
  }

  ngOnInit(): void {
    // Set page title
    this.titleService.setTitle(environment.appTitle);
    
    // Update authentication status
    this.authService.isAuthenticated$.subscribe(
      isAuthenticated => {
        this.isAuthenticated = isAuthenticated;
      }
    );
    
    // Track router events for analytics or other purposes
    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd)
    ).subscribe(() => {
      // You could add analytics tracking here
      window.scrollTo(0, 0);
    });
  }

  switchLanguage(language: string): void {
    this.translate.use(language);
    this.isRtl = language.startsWith('ar');
    document.documentElement.dir = this.isRtl ? 'rtl' : 'ltr';
    document.documentElement.lang = language;
  }

  logout(): void {
    this.authService.logout();
  }
}
