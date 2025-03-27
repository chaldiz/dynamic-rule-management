import { ModelField } from "./model-field";
import { ValidationRule } from "./validation-rule";

export interface DynamicModel {
    id?: number;
    name: string;
    description: string;
    fields: ModelField[];
    modelValidationRules: ValidationRule[];
  }
  