import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, FormArray, Validators, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { DynamicModelService } from '../../services/dynamic-model.service';
import { ValidationRule } from '../../models/validation-rule';
import { ModelField } from '../../models/model-field';
import { DynamicModel } from '../../models/dynamic-model';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-model-form',
  templateUrl: './model-form.component.html',
  styleUrls: ['./model-form.component.scss'],
  standalone: true,
  imports: [
    CommonModule, 
    ReactiveFormsModule
  ],
})
export class ModelFormComponent implements OnInit {
  modelForm: FormGroup;
  modelId: number | null = null;
  isEditMode = false;
  loading = false;
  submitting = false;
  error: string | null = null;
  dataTypes = ['String', 'Int', 'Decimal', 'DateTime', 'Boolean', 'Object', 'Array'];
  validationRuleTypes = [
    { name: 'Required', modelLevel: false, fieldLevel: true },
    { name: 'MinLength', modelLevel: false, fieldLevel: true },
    { name: 'MaxLength', modelLevel: false, fieldLevel: true },
    { name: 'Pattern', modelLevel: false, fieldLevel: true },
    { name: 'Range', modelLevel: false, fieldLevel: true },
    { name: 'Email', modelLevel: false, fieldLevel: true },
    { name: 'OneOf', modelLevel: false, fieldLevel: true },
    { name: 'RequiredFields', modelLevel: true, fieldLevel: false },
    { name: 'ConditionalRequired', modelLevel: true, fieldLevel: false }
  ];

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private modelService: DynamicModelService
  ) {
    this.modelForm = this.createModelForm();
  }

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    
    if (id && id !== 'new') {
      this.modelId = +id;
      this.isEditMode = true;
      this.loadModel(this.modelId);
    }
  }

  createModelForm(): FormGroup {
    return this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(100)]],
      description: ['', Validators.maxLength(500)],
      modelValidationRules: this.fb.array([]),
      fields: this.fb.array([])
    });
  }

  get modelValidationRules(): FormArray {
    return this.modelForm.get('modelValidationRules') as FormArray;
  }

  get fields(): FormArray {
    return this.modelForm.get('fields') as FormArray;
  }

  createModelValidationRuleForm(): FormGroup {
    return this.fb.group({
      ruleName: ['', Validators.required],
      errorMessage: ['', Validators.required],
      parameters: this.fb.group({})
    });
  }

  createFieldForm(): FormGroup {
    return this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(100)]],
      dataType: ['String', Validators.required],
      description: ['', Validators.maxLength(200)],
      isRequired: [false],
      maxLength: [null],
      defaultValue: [''],
      displayOrder: [0, [Validators.required, Validators.min(0)]],
      nestedModelId: [null],
      validationRules: this.fb.array([])
    });
  }

  createFieldValidationRuleForm(): FormGroup {
    return this.fb.group({
      ruleName: ['', Validators.required],
      errorMessage: ['', Validators.required],
      parameters: this.fb.group({})
    });
  }

  addModelValidationRule(): void {
    this.modelValidationRules.push(this.createModelValidationRuleForm());
  }

  addField(): void {
    this.fields.push(this.createFieldForm());
  }

  addFieldValidationRule(fieldIndex: number): void {
    const validationRules = this.fields.at(fieldIndex).get('validationRules') as FormArray;
    validationRules.push(this.createFieldValidationRuleForm());
  }

  removeModelValidationRule(index: number): void {
    this.modelValidationRules.removeAt(index);
  }

  removeField(index: number): void {
    this.fields.removeAt(index);
  }

  removeFieldValidationRule(fieldIndex: number, ruleIndex: number): void {
    const validationRules = this.fields.at(fieldIndex).get('validationRules') as FormArray;
    validationRules.removeAt(ruleIndex);
  }

  getFieldValidationRules(fieldIndex: number): FormArray {
    return this.fields.at(fieldIndex).get('validationRules') as FormArray;
  }

  updateRuleParametersForm(ruleForm: FormGroup, ruleName: string): void {
    const parametersGroup = ruleForm.get('parameters') as FormGroup;
    
    // Clear existing parameters
    Object.keys(parametersGroup.controls).forEach(key => {
      parametersGroup.removeControl(key);
    });
    
    // Add parameters based on rule type
    switch (ruleName) {
      case 'MinLength':
        parametersGroup.addControl('min', this.fb.control('', Validators.required));
        break;
      case 'MaxLength':
        parametersGroup.addControl('max', this.fb.control('', Validators.required));
        break;
      case 'Pattern':
        parametersGroup.addControl('regex', this.fb.control('', Validators.required));
        break;
      case 'Range':
        parametersGroup.addControl('min', this.fb.control('', Validators.required));
        parametersGroup.addControl('max', this.fb.control('', Validators.required));
        break;
      case 'OneOf':
        parametersGroup.addControl('values', this.fb.control('', Validators.required));
        break;
      case 'RequiredFields':
        parametersGroup.addControl('fields', this.fb.control('', Validators.required));
        break;
      case 'ConditionalRequired':
        parametersGroup.addControl('ifField', this.fb.control('', Validators.required));
        parametersGroup.addControl('ifValue', this.fb.control('', Validators.required));
        parametersGroup.addControl('thenFields', this.fb.control('', Validators.required));
        break;
    }
  }

  loadModel(id: number): void {
    this.loading = true;
    this.error = null;
    
    this.modelService.getModelById(id).subscribe({
      next: (model) => {
        this.patchModelForm(model);
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Model yüklenirken hata oluştu: ' + err.message;
        this.loading = false;
      }
    });
  }

  patchModelForm(model: DynamicModel): void {
    // Reset form
    this.modelForm.reset();
    this.modelValidationRules.clear();
    this.fields.clear();
    
    // Patch basic model info
    this.modelForm.patchValue({
      name: model.name,
      description: model.description
    });
    
    // Add model validation rules
    if (model.modelValidationRules && model.modelValidationRules.length > 0) {
      model.modelValidationRules.forEach(rule => {
        const ruleForm = this.createModelValidationRuleForm();
        ruleForm.patchValue({
          ruleName: rule.ruleName,
          errorMessage: rule.errorMessage
        });
        
        this.updateRuleParametersForm(ruleForm, rule.ruleName);
        const parametersGroup = ruleForm.get('parameters') as FormGroup;
        
        // Patch parameter values
        if (rule.parameters) {
          Object.keys(rule.parameters).forEach(key => {
            if (parametersGroup.contains(key)) {
              parametersGroup.get(key)?.setValue(rule.parameters[key]);
            }
          });
        }
        
        this.modelValidationRules.push(ruleForm);
      });
    }
    
    // Add fields
    if (model.fields && model.fields.length > 0) {
      model.fields.forEach(field => {
        const fieldForm = this.createFieldForm();
        fieldForm.patchValue({
          name: field.name,
          dataType: field.dataType,
          description: field.description,
          isRequired: field.isRequired,
          maxLength: field.maxLength,
          defaultValue: field.defaultValue,
          displayOrder: field.displayOrder,
          nestedModelId: field.nestedModelId
        });
        
        // Add field validation rules
        const validationRulesArray = fieldForm.get('validationRules') as FormArray;
        
        if (field.validationRules && field.validationRules.length > 0) {
          field.validationRules.forEach(rule => {
            const ruleForm = this.createFieldValidationRuleForm();
            ruleForm.patchValue({
              ruleName: rule.ruleName,
              errorMessage: rule.errorMessage
            });
            
            this.updateRuleParametersForm(ruleForm, rule.ruleName);
            const parametersGroup = ruleForm.get('parameters') as FormGroup;
            
            // Patch parameter values
            if (rule.parameters) {
              Object.keys(rule.parameters).forEach(key => {
                if (parametersGroup.contains(key)) {
                  parametersGroup.get(key)?.setValue(rule.parameters[key]);
                }
              });
            }
            
            validationRulesArray.push(ruleForm);
          });
        }
        
        this.fields.push(fieldForm);
      });
    }
  }

  onModelRuleTypeChange(index: number): void {
    const ruleForm = this.modelValidationRules.at(index);
    const ruleName = ruleForm.get('ruleName')?.value;
    
    if (ruleName) {
      this.updateRuleParametersForm(ruleForm as FormGroup, ruleName);
    }
  }

  onFieldRuleTypeChange(fieldIndex: number, ruleIndex: number): void {
    const validationRules = this.getFieldValidationRules(fieldIndex);
    const ruleForm = validationRules.at(ruleIndex);
    const ruleName = ruleForm.get('ruleName')?.value;
    
    if (ruleName) {
      this.updateRuleParametersForm(ruleForm as FormGroup, ruleName);
    }
  }

  parseParameterValues(parametersFormValue: any): { [key: string]: any } {
    const result: { [key: string]: any } = {};
    
    Object.keys(parametersFormValue).forEach(key => {
      const value = parametersFormValue[key];
      
      // Parse array values
      if (key === 'values' || key === 'fields' || key === 'thenFields') {
        result[key] = value.split(',').map((item: string) => item.trim());
      } 
      // Parse numeric values
      else if (key === 'min' || key === 'max') {
        result[key] = parseFloat(value);
      } 
      // Keep other values as is
      else {
        result[key] = value;
      }
    });
    
    return result;
  }

  formatFormValue(): DynamicModel {
    const formValue = this.modelForm.value;
    
    // Process model validation rules
    const modelValidationRules: ValidationRule[] = formValue.modelValidationRules.map((rule: any) => ({
      ruleName: rule.ruleName,
      errorMessage: rule.errorMessage,
      parameters: this.parseParameterValues(rule.parameters)
    }));
    
    // Process fields
    const fields: ModelField[] = formValue.fields.map((field: any) => {
      // Process field validation rules
      const validationRules: ValidationRule[] = field.validationRules.map((rule: any) => ({
        ruleName: rule.ruleName,
        errorMessage: rule.errorMessage,
        parameters: this.parseParameterValues(rule.parameters)
      }));
      
      return {
        name: field.name,
        dataType: field.dataType,
        description: field.description,
        isRequired: field.isRequired,
        maxLength: field.maxLength,
        defaultValue: field.defaultValue,
        displayOrder: field.displayOrder,
        nestedModelId: field.nestedModelId,
        validationRules
      };
    });
    
    return {
      id: this.modelId || undefined,
      name: formValue.name,
      description: formValue.description,
      modelValidationRules,
      fields
    };
  }

  onSubmit(): void {
    if (this.modelForm.invalid) {
      // Mark all fields as touched to trigger validation messages
      this.markFormGroupTouched(this.modelForm);
      return;
    }
    
    const model = this.formatFormValue();
    this.submitting = true;
    this.error = null;
    
    if (this.isEditMode && this.modelId) {
      // Update existing model
      this.modelService.updateModel(this.modelId, model).subscribe({
        next: () => {
          this.submitting = false;
          this.router.navigate(['/models']);
        },
        error: (err) => {
          this.error = 'Model güncellenirken hata oluştu: ' + err.message;
          this.submitting = false;
        }
      });
    } else {
      // Create new model
      this.modelService.createModel(model).subscribe({
        next: () => {
          this.submitting = false;
          this.router.navigate(['/models']);
        },
        error: (err) => {
          this.error = 'Model oluşturulurken hata oluştu: ' + err.message;
          this.submitting = false;
        }
      });
    }
  }

  markFormGroupTouched(formGroup: FormGroup | FormArray): void {
    Object.keys(formGroup.controls).forEach(key => {
      const control = formGroup.get(key);
      
      if (control instanceof FormGroup || control instanceof FormArray) {
        this.markFormGroupTouched(control);
      } else {
        control?.markAsTouched();
      }
    });
  }

  cancel(): void {
    this.router.navigate(['/models']);
  }
}