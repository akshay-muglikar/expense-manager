import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { trigger, transition, style, animate, stagger, query } from '@angular/animations';
import { Router } from '@angular/router';

@Component({
  selector: 'app-landing',
  standalone: true,
  imports: [
    CommonModule,
    MatButtonModule,
    MatIconModule,
    MatCardModule
  ],
  templateUrl: './landing.component.html',
  styleUrls: ['./landing.component.scss'],
  animations: [
    trigger('fadeIn', [
      transition(':enter', [
        style({ opacity: 0, transform: 'translateY(20px)' }),
        animate('600ms ease-out', style({ opacity: 1, transform: 'translateY(0)' }))
      ])
    ]),
    trigger('staggerFadeIn', [
      transition('* => *', [
        query(':enter', [
          style({ opacity: 0, transform: 'translateY(20px)' }),
          stagger('200ms', [
            animate('400ms ease-out', style({ opacity: 1, transform: 'translateY(0)' }))
          ])
        ], { optional: true })
      ])
    ])
  ]
})
export class LandingComponent {

    constructor(private router: Router) {}
    bookDemo() {
        this.router.navigate(['/book-demo']);
    }
  features = [
    {
      icon: 'inventory_2',
      title: 'Inventory Management',
      description: 'Track stock levels, manage multiple warehouses, and automate reordering with real-time updates and alerts.'
    },
    {
      icon: 'receipt_long',
      title: 'Smart Billing',
      description: 'Create professional invoices, manage payments, and track receivables with our intelligent billing system.'
    },
    {
      icon: 'insights',
      title: 'Real-time Analytics',
      description: 'Make data-driven decisions with advanced reporting, customizable dashboards, and business intelligence tools.'
    },
    {
      icon: 'point_of_sale',
      title: 'POS Integration',
      description: 'Seamless integration with point of sale systems for retail operations with real-time synchronization.'
    },
    {
      icon: 'account_balance',
      title: 'Expense Tracking',
      description: 'Monitor expenses and generate detailed financial reports'
    },
    {
      icon: 'store',
      title: 'Multi-Client Support',
      description: 'Perfect for businesses with multiple branches or clients'
    },
    {
      icon: 'smart_toy',
      title: 'AI Business Assistant',
      description: 'Coming Soon! Your intelligent business companion. Get smart inventory suggestions, automated reordering recommendations, and predictive analytics powered by advanced AI.',
      isComingSoon: true
    },
    {
        icon: 'support_agent',
        title: '24/7 Support',
        description: 'Our dedicated support team is available around the clock to assist you with any issues or questions.'
    },
    {
    icon: 'settings_suggest',
    title: 'Customizable Solutions',
    description: 'First in the industry to offer fully customizable features. We adapt our software to your unique business needs with rapid updates and personalized solutions.',
        isfirst: true
    }
    
  ];

  testimonial = {
    clientName: 'Sai Car Decor',
    quote: 'This system has transformed how we manage our inventory and billing. The efficiency gains are remarkable.',
    role: 'Auto Accessories & Customization',
    logo: 'logo.png'
  };
}
