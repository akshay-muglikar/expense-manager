import { Component, ElementRef, ViewChild, AfterViewChecked, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatCardModule } from '@angular/material/card';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { Subscription } from 'rxjs';
import { AIService, AIRequest } from '../services/ai.service';

export interface ChatMessage {
  id: number;
  content: string;
  isUser: boolean;
  timestamp: Date;
  suggestions: string[]
}

@Component({
  selector: 'app-shrivo-chat',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatIconModule,
    MatButtonModule,
    MatInputModule,
    MatFormFieldModule,
    MatCardModule,
    MatProgressBarModule
  ],
  templateUrl: './shrivo-chat.component.html',
  styleUrl: './shrivo-chat.component.scss'
})
export class ShrivoChatComponent implements AfterViewChecked, OnInit, OnDestroy {
  @ViewChild('messagesContainer') private messagesContainer!: ElementRef;
  
  messages: ChatMessage[] = [];
  
  currentMessage = '';
  isTyping = false;
  isMinimized = true;
  isHealthy = false;
  errorMessage = '';
  quickActions: string[] = [
    'How to add inventory?',
    'How to create a bill?',
    'How to manage customers?',
    'How to add vendors?'
  ];
  healthCheckSubscription?: Subscription;

  constructor(private aiService: AIService) {}

  ngOnInit() {
    this.checkAIHealth();
    this.loadDefaultQuestions();
    this.startHealthMonitoring();
  }

  ngOnDestroy() {
    this.aiService.stopHealthMonitoring();
    if (this.healthCheckSubscription) {
      this.healthCheckSubscription.unsubscribe();
    }
  }

  // Check AI backend health
  checkAIHealth() {
    this.aiService.checkHealth().subscribe(result => {
      this.isHealthy = result.isHealthy;
      this.errorMessage = result.errorMessage;
    });
  }

  // Start periodic health monitoring
  private startHealthMonitoring() {
    this.healthCheckSubscription = this.aiService.startHealthMonitoring(
      (isHealthy: boolean, errorMessage: string) => {
        this.isHealthy = isHealthy;
        this.errorMessage = errorMessage;
      }
    );
  }

  // Load default questions from backend
  private loadDefaultQuestions() {
    this.aiService.getSuggestions().subscribe(suggestions => {
      this.quickActions = suggestions.length > 0 ? suggestions : this.getDefaultSuggestions(null);
      // push element at index 3
    });
  }

  // Get default fallback suggestions
  private getDefaultSuggestions(currentMesage:string|null): string[] {
    return [
      'How to add inventory?',
      'How to create a bill?',
      'Help with expenses',
      'Troubleshooting help',
      'Best practices'
    ].filter(x=> !(x == currentMesage));
  }

  // Send message to AI backend
  private sendToAI(query: string): void {
    if (!this.isHealthy) {
      this.addMessage({
        id: Date.now(),
        content: 'Sorry, the AI service is currently unavailable. Please try again later.',
        isUser: false,
        timestamp: new Date(),
        suggestions:[]
      });
      return;
    }

    const request: AIRequest = {
      query: query,
      context: this.getConversationContext(),
      includeLocalData: true
    };

    this.aiService.askQuestion(request).subscribe(response => {
      this.isTyping = false;
      this.addMessage({
        id: Date.now(),
        content: response.response,
        isUser: false,
        timestamp: new Date(),
        suggestions:[]
      });
    });
  }

  // Get conversation context for AI
  private getConversationContext(): string {
    const recentMessages = this.messages.slice(-6); // Last 6 messages for context
    return recentMessages.map(msg => 
      `${msg.isUser ? 'User' : 'AI'}: ${msg.content}`
    ).join('\n');
  }
  private quickResponses: { [key: string]: string } = {
    'add inventory': `📦 **Adding New Inventory Items:**

1. Navigate to **Bills** section
2. Click **"Add new Inventory"** button  
3. Fill in the required details:
   - Item name (required)
   - Category
   - Car/Vehicle type
   - Quantity
   - Description
   - Price
   - Barcode (optional)
4. Click **Save** to add the item

💡 **Tip:** Items appear immediately in billing after saving!`,

    'create bill': `🧾 **Creating a New Bill:**

1. Go to **Bills** section from sidebar
2. Enter customer information:
   - Customer name (required)
   - Mobile number (for WhatsApp)
3. Add items using:
   - Search and click item cards
   - Barcode scanner
   - Dropdown selection
4. Adjust quantities and prices
5. Select payment mode (Cash/Card/UPI)
6. Apply discount/advance if needed
7. Click **"Save Bill"**

🎯 **Quick tip:** Use barcode scanning for faster item selection!`,

    'add vendor': `🏭 **Adding New Vendor:**

1. Navigate to **Vendors** section
2. Fill in the vendor form:
   - Vendor name (required)
   - Mobile number (required)
3. Click **"Add Vendor"**
4. Vendor appears in cards view
5. Ready for expense tracking!

💰 **Next step:** Click on vendor card to add expenses and track account balance.`,

    'whatsapp': `📱 **Sending Bills via WhatsApp:**

1. Create and **save the bill** first
2. Click the **WhatsApp icon** in bill actions
3. System generates message with:
   - Bill details
   - Total amount
   - Download link
4. WhatsApp opens with pre-filled message
5. Send directly to customer

⚠️ **Note:** Customer mobile number must be entered for this feature to work.`,

    'barcode': `🔍 **Using Barcode Scanner:**

1. In billing section, find the barcode input field
2. Scan barcode or type manually
3. Press **Enter** or click scan
4. Item automatically adds to bill
5. Adjust quantity if needed

💡 **Tips:**
- Ensure items have barcodes assigned in inventory
- Scanner input stays focused for multiple scans
- Use for faster checkout process`,

    'help': `🎯 **I can help you with:**

**📦 Inventory Management:**
- Adding new items
- Updating prices & quantities
- Barcode management

**🧾 Billing & Invoicing:**
- Creating bills
- WhatsApp integration
- PDF generation
- Bill editing

**🏭 Vendor Management:**
- Adding suppliers
- Tracking expenses
- Account balances

**👥 Customer Management:**
- Customer database
- Purchase history
- Quick billing

**💰 Expense Tracking:**
- Recording expenses
- Financial reporting

Just ask me anything like "how to add inventory" or "create bill"!`
  };

  ngAfterViewChecked() {
    this.scrollToBottom();
  }

  toggleChat() {
    this.isMinimized = !this.isMinimized;
  }

  sendMessage() {
    if (!this.currentMessage.trim()) return;
    if(this.currentMessage.trim().toUpperCase() == "HELP"){
       this.addMessage({
        id: Date.now(),
        content: 'What can I help you with.',
        suggestions : this.getDefaultSuggestions(null),
        isUser: false,
        timestamp: new Date(),
      });
      this.currentMessage = '';
      return
    }
    if(this.getDefaultSuggestions(null).indexOf(this.currentMessage.trim())!=-1){
        let message = this.generateResponse(this.currentMessage);
        this.addMessage({
        id: Date.now(),
        content: message,
        suggestions :this.getDefaultSuggestions(this.currentMessage),
        isUser: false,
        timestamp: new Date(),
      });
      this.currentMessage = '';
        return
    }
    // Add user message
    const userMessage: ChatMessage = {
      id: this.messages.length + 1,
      content: this.currentMessage.trim(),
      isUser: true,
      timestamp: new Date(),
      suggestions:[]
    };
    this.messages.push(userMessage);

    const messageText = this.currentMessage.trim();
    this.currentMessage = '';
    this.isTyping = true;

    // Send to AI backend instead of generating local response
    this.sendToAI(messageText);
  }

  // Helper method to add messages
  private addMessage(message: ChatMessage): void {
    this.messages.push(message);
  }

  private generateResponse(message: string): string {
    // Check for exact matches first
    for (const [key, response] of Object.entries(this.quickResponses)) {
      if (message.includes(key)) {
        return response;
      }
    }

    // Pattern matching for common queries
    if (message.includes('inventory') || message.includes('item') || message.includes('product') || message.includes('stock')) {
      return this.quickResponses['add inventory'];
    }

    if (message.includes('bill') || message.includes('invoice') || message.includes('sale') || message.includes('receipt')) {
      return this.quickResponses['create bill'];
    }

    if (message.includes('vendor') || message.includes('supplier') || message.includes('vendor expense')) {
      return this.quickResponses['add vendor'];
    }

    if (message.includes('whatsapp') || message.includes('send') || message.includes('share') || message.includes('message')) {
      return this.quickResponses['whatsapp'];
    }

    if (message.includes('scan') || message.includes('barcode') || message.includes('code')) {
      return this.quickResponses['barcode'];
    }

    if (message.includes('customer') || message.includes('client') || message.includes('buyer')) {
      return `👥 **Customer Management:**

1. Go to **Customers** section
2. Browse or search existing customers
3. View customer details:
   - Purchase history
   - Total bills
   - Last purchase date
4. Click **"Create Bill"** for quick billing

🔍 **Search:** Use name or mobile number to find customers quickly.
💡 **Tip:** Customer mobile numbers enable WhatsApp integration!`;
    }

    if (message.includes('expense') || message.includes('cost') || message.includes('spending') || message.includes('money')) {
      return `💰 **Managing Expenses:**

1. Navigate to **Expense** section
2. Fill in expense details:
   - Description
   - Amount
   - Category
   - Date
   - Payment method
3. Link to vendor (optional)
4. Save expense

📊 **Filtering:** Use date ranges and categories to analyze spending patterns.
💡 **Integration:** Link expenses to vendors for better account tracking!`;
    }

    if (message.includes('error') || message.includes('problem') || message.includes('issue') || message.includes('not working') || message.includes('broken')) {
      return `🔧 **Common Issues & Solutions:**

**📱 Items not appearing in search:**
- Check spelling in search terms
- Verify item exists in inventory
- Try partial name or category search
- Refresh the page

**💳 Payment/billing issues:**
- Verify payment mode is selected
- Ensure bill is saved before sharing
- Check all required fields are filled

**📊 Wrong calculations:**
- Verify quantity and price fields
- Check discount/advance amounts
- Refresh page if calculations seem stuck

**🔄 Data syncing problems:**
- Check internet connection
- Try refreshing the page
- Clear browser cache if persistent

**📱 WhatsApp not working:**
- Ensure customer mobile number is entered
- Save bill before attempting to send
- Check WhatsApp is installed on device

Need help with a specific error? Describe exactly what's happening and I'll provide targeted help!`;
    }

    if (message.includes('tip') || message.includes('best') || message.includes('practice') || message.includes('optimize') || message.includes('improve')) {
      return `💡 **Pro Tips & Best Practices:**

**📦 Inventory Management:**
- Use consistent naming conventions (e.g., "Brake Pad - Honda Civic")
- Set up barcodes for frequently sold items
- Regular inventory audits to verify stock levels
- Stock quantities are automatically updated after billing

**🧾 Billing Efficiency:**
- Always collect customer mobile numbers for WhatsApp
- Use barcode scanning for speed and accuracy
- Save bills before printing or sharing

**💰 Financial Management:**
- Record all expenses promptly with proper descriptions
- Monitor vendor account balances regularly
- Use date filters for financial reporting
- Link expenses to specific vendors when possible


**📱 Mobile Usage:**
- App is fully responsive for mobile use
- Touch-friendly interface for tablet operations
- WhatsApp integration works seamlessly on mobile

Want specific tips for any particular module or workflow?`;
    }

    if (message.includes('hello') || message.includes('hi') || message.includes('hey')) {
      return `👋 Hello! I'm Shrivo, your AI assistant for the inventory management system.

I'm here to help you with:
• **Quick guidance** on using features
• **Step-by-step instructions** for common tasks  
• **Troubleshooting** any issues you encounter
• **Best practices** to optimize your workflow
• **Tips and tricks** to work more efficiently

Just ask me anything like:
- "How do I add new inventory?"
- "How to send bills via WhatsApp?"
- "Help with vendor management"
- "Best practices for billing"

What would you like help with today? 😊`;
    }

    if (message.includes('thank') || message.includes('thanks')) {
      return `😊 You're very welcome! I'm glad I could help.

Feel free to ask me anything else about:
• Adding and managing inventory
• Creating and sharing bills
• Vendor and customer management  
• Expense tracking
• Troubleshooting issues
• Tips for better efficiency

I'm always here to assist you with the inventory management system! 🚀`;
    }

    // Default response for unrecognized queries
    return `🤔 I'm not sure about that specific query, but I'm here to help! 

I can assist with:
• **Inventory management** - adding items, updating stock, barcode setup
• **Billing processes** - creating invoices, WhatsApp sharing, payment tracking
• **Vendor management** - adding suppliers, tracking expenses, account balances  
• **Customer management** - customer database, purchase history, quick billing
• **Expense tracking** - recording costs, financial reports, vendor integration

**Try asking:**
- "How to add new inventory?"
- "How to create a bill?"  
- "How to add vendor expenses?"
- "How to send bill via WhatsApp?"
- "Best practices" or "troubleshooting help"

Or just say **"help"** to see all available topics! 😊`;
  }

  private scrollToBottom(): void {
    try {
      if (this.messagesContainer) {
        this.messagesContainer.nativeElement.scrollTop = this.messagesContainer.nativeElement.scrollHeight;
      }
    } catch (err) {
      console.error('Error scrolling to bottom:', err);
    }
  }

  onKeyPress(event: KeyboardEvent) {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      this.sendMessage();
    }
  }

  clearChat() {
    this.messages = [];
    // Welcome message will automatically display when messages.length === 0 and isHealthy
  }

  // Additional helper methods
  trackByMessage(index: number, message: ChatMessage): number {
    return message.id;
  }

  formatMessage(content: string): string {
    // Convert markdown-like formatting to HTML
    return content
      .replace(/\*\*(.*?)\*\*/g, '<strong>$1</strong>')
      .replace(/\*(.*?)\*/g, '<em>$1</em>')
      .replace(/\n/g, '<br>')
      .replace(/•/g, '&bull;');
  }

  formatTime(timestamp: Date): string {
    return timestamp.toLocaleTimeString([], { 
      hour: '2-digit', 
      minute: '2-digit' 
    });
  }

  sendQuickMessage(message: string) {
    this.currentMessage = message;
    this.sendMessage();
  }

  // Helper methods for quick actions
  trackByAction(index: number, action: string): string {
    return action;
  }

  getQuickActionIcon(action: string): string {
    const iconMap: { [key: string]: string } = {
      'help': 'help',
      'add inventory': 'inventory',
      'create bill': 'receipt',
      'view reports': 'analytics',
      'manage customers': 'people',
      'expenses': 'account_balance_wallet',
      'dashboard': 'dashboard'
    };
    
    const lowerAction = action.toLowerCase();
    for (const key in iconMap) {
      if (lowerAction.includes(key.toLowerCase())) {
        return iconMap[key];
      }
    }
    return 'chat'; // default icon
  }

  formatQuickActionText(action: string): string {
    // Capitalize first letter and format text
    return action.charAt(0).toUpperCase() + action.slice(1);
  }
}
