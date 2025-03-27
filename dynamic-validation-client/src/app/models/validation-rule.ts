export interface ValidationRule {
    id?: number;
    ruleName: string;
    errorMessage: string;
    parameters: { [key: string]: any };
  }