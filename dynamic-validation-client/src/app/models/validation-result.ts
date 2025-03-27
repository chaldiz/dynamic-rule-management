import { ValidationError } from "./validation-error";

export interface ValidationResult {
    isValid: boolean;
    modelName: string;
    errors: ValidationError[];
  }