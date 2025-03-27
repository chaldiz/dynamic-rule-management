import { TestBed } from '@angular/core/testing';

import { DynamicModelService } from './dynamic-model.service';

describe('DynamicModelService', () => {
  let service: DynamicModelService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(DynamicModelService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
