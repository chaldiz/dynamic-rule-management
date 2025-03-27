// validation-test.component.ts
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormBuilder, FormGroup, FormControl, ReactiveFormsModule } from '@angular/forms';
import { DynamicModelService } from '../../services/dynamic-model.service';
import { DynamicModel } from '../../models/dynamic-model';
import { ValidationResult } from '../../models/validation-result';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-validation-test',
  templateUrl: './validation-test.component.html',
  styleUrls: ['./validation-test.component.scss'],
  standalone: true,
  imports: [
    CommonModule, 
    ReactiveFormsModule
  ],
})
export class ValidationTestComponent implements OnInit {
  model: DynamicModel | null = null;
  modelId: number = 0;
  testForm: FormGroup;
  jsonInput: FormControl = new FormControl('{}');
  useJsonEditor = false;
  loading = false;
  validating = false;
  validationResult: ValidationResult | null = null;
  error: string | null = null;

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private modelService: DynamicModelService
  ) {
    this.testForm = this.fb.group({});
  }

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.modelId = +id;
      this.loadModel(this.modelId);
    }
  }

  loadModel(id: number): void {
    this.loading = true;
    this.error = null;
    
    this.modelService.getModelById(id).subscribe({
      next: (model) => {
        this.model = model;
        this.initializeForm(model);
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Model yüklenirken hata oluştu: ' + err.message;
        this.loading = false;
      }
    });
  }

  initializeForm(model: DynamicModel): void {
    // Clear existing form
    this.testForm = this.fb.group({});
    
    // Create form controls for each field
    if (model.fields && model.fields.length > 0) {
      model.fields.sort((a, b) => a.displayOrder - b.displayOrder);
      
      model.fields.forEach(field => {
        // Initialize default value based on field type
        let defaultValue: any = field.defaultValue || '';
        
        switch (field.dataType) {
          case 'Int':
            defaultValue = field.defaultValue ? parseInt(field.defaultValue) : null;
            break;
          case 'Decimal':
            defaultValue = field.defaultValue ? parseFloat(field.defaultValue) : null;
            break;
          case 'Boolean':
            defaultValue = field.defaultValue === 'true';
            break;
          case 'Object':
            defaultValue = field.defaultValue ? JSON.parse(field.defaultValue) : {};
            break;
          case 'Array':
            defaultValue = field.defaultValue ? JSON.parse(field.defaultValue) : [];
            break;
        }
        
        this.testForm.addControl(field.name, new FormControl(defaultValue));
      });
    }
    
    // Update JSON editor with default form values
    this.updateJsonEditor();
  }

  toggleEditor(): void {
    this.useJsonEditor = !this.useJsonEditor;
    
    if (this.useJsonEditor) {
      this.updateJsonEditor();
    } else {
      this.updateFormFromJson();
    }
  }

  updateJsonEditor(): void {
    const formValue = this.testForm.value;
    this.jsonInput.setValue(JSON.stringify(formValue, null, 2));
  }

  updateFormFromJson(): void {
    try {
      const jsonValue = JSON.parse(this.jsonInput.value);
      
      // Update form controls with JSON values
      Object.keys(jsonValue).forEach(key => {
        if (this.testForm.contains(key)) {
          this.testForm.get(key)?.setValue(jsonValue[key]);
        }
      });
    } catch (error) {
      this.error = 'Geçersiz JSON formatı';
    }
  }

  onFieldChange(): void {
    if (this.useJsonEditor) {
      this.updateJsonEditor();
    }
  }

  onJsonChange(): void {
    if (this.error) {
      this.error = null;
    }
  }

  validateData(): void {
    this.validating = true;
    this.error = null;
    this.validationResult = null;
    
    let data: any;
    
    try {
      if (this.useJsonEditor) {
        data = JSON.parse(this.jsonInput.value);
      } else {
        data = this.testForm.value;
      }
      
      if (!this.model) {
        throw new Error('Model yüklenmedi');
      }
      
      this.modelService.validateData(this.model.name, data).subscribe({
        next: (result) => {
          this.validationResult = result;
          this.validating = false;
        },
        error: (err) => {
          this.error = 'Validasyon sırasında hata oluştu: ' + err.message;
          this.validating = false;
        }
      });
    } catch (error: any) {
      this.error = 'Geçersiz veri formatı: ' + error.message;
      this.validating = false;
    }
  }

  getFieldType(dataType: string): string {
    switch (dataType) {
      case 'Int':
      case 'Decimal':
        return 'number';
      case 'Boolean':
        return 'checkbox';
      case 'DateTime':
        return 'datetime-local';
      default:
        return 'text';
    }
  }

  goBack(): void {
    this.router.navigate(['/models/view', this.modelId]);
  }
}