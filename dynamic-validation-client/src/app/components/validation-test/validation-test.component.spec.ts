import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ValidationTestComponent } from './validation-test.component';

describe('ValidationTestComponent', () => {
  let component: ValidationTestComponent;
  let fixture: ComponentFixture<ValidationTestComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ValidationTestComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ValidationTestComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
