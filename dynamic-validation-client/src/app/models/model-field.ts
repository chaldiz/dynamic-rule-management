import { ValidationRule } from "./validation-rule";

export interface ModelField {
    id?: number;
    name: string;
    dataType: string;
    description: string;
    isRequired: boolean;
    maxLength?: number;
    defaultValue?: string;
    displayOrder: number;
    nestedModelId?: number;
    validationRules: ValidationRule[];
  }