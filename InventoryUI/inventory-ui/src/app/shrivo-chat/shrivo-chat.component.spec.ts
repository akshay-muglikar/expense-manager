import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ShrivoChatComponent } from './shrivo-chat.component';

describe('ShrivoChatComponent', () => {
  let component: ShrivoChatComponent;
  let fixture: ComponentFixture<ShrivoChatComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ShrivoChatComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ShrivoChatComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should start minimized', () => {
    expect(component.isMinimized).toBe(true);
  });

  it('should have welcome message', () => {
    expect(component.messages.length).toBe(1);
    expect(component.messages[0].content).toContain('Hi! I\'m Shrivo');
  });

  it('should toggle chat visibility', () => {
    const initialState = component.isMinimized;
    component.toggleChat();
    expect(component.isMinimized).toBe(!initialState);
  });

  it('should send message', () => {
    component.currentMessage = 'test message';
    const initialLength = component.messages.length;
    component.sendMessage();
    expect(component.messages.length).toBe(initialLength + 1);
    expect(component.currentMessage).toBe('');
  });

  it('should generate appropriate response', () => {
    component.currentMessage = 'how to add inventory';
    component.sendMessage();
    setTimeout(() => {
      expect(component.messages[component.messages.length - 1].content).toContain('Adding New Inventory Items');
    }, 1100);
  });

  it('should clear chat', () => {
    component.messages.push({
      id: 99,
      content: 'test',
      isUser: true,
      timestamp: new Date(),
      suggestions :[]
    });
    component.clearChat();
    expect(component.messages.length).toBe(1);
    expect(component.messages[0].content).toContain('Chat cleared!');
  });
});
