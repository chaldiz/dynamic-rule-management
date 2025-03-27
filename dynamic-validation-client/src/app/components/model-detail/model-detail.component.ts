import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { DynamicModelService } from '../../services/dynamic-model.service';
import { DynamicModel } from '../../models/dynamic-model';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';

@Component({
  selector: 'app-model-detail',
  templateUrl: './model-detail.component.html',
  styleUrls: ['./model-detail.component.scss'],
  standalone: true,
  imports: [
    CommonModule, 
    ReactiveFormsModule
  ],
})
export class ModelDetailComponent implements OnInit {
  model: DynamicModel | null = null;
  modelId: number = 0;
  loading = false;
  error: string | null = null;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private modelService: DynamicModelService
  ) { }

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
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Model yüklenirken hata oluştu: ' + err.message;
        this.loading = false;
      }
    });
  }

  editModel(): void {
    this.router.navigate(['/models/edit', this.modelId]);
  }

  testModel(): void {
    this.router.navigate(['/validation-test', this.modelId]);
  }

  goBack(): void {
    this.router.navigate(['/models']);
  }
}