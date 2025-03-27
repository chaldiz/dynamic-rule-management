import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { DynamicModelService } from '../../services/dynamic-model.service';
import { DynamicModel } from '../../models/dynamic-model';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';

@Component({
  selector: 'app-model-list',
  templateUrl: './model-list.component.html',
  styleUrls: ['./model-list.component.scss'],
  standalone: true,
  imports: [
    CommonModule, 
    ReactiveFormsModule
  ],
})
export class ModelListComponent implements OnInit {
  models: DynamicModel[] = [];
  loading = false;
  error: string | null = null;

  constructor(
    private modelService: DynamicModelService,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.loadModels();
  }

  loadModels(): void {
    this.loading = true;
    this.error = null;
    
    this.modelService.getModels().subscribe({
      next: (data) => {
        this.models = data;
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Model listesi yüklenirken hata oluştu: ' + err.message;
        this.loading = false;
      }
    });
  }

  createNewModel(): void {
    this.router.navigate(['/models/new']);
  }

  editModel(id: number): void {
    this.router.navigate(['/models/edit', id]);
  }

  deleteModel(id: number): void {
    if (confirm('Bu modeli silmek istediğinizden emin misiniz?')) {
      this.modelService.deleteModel(id).subscribe({
        next: () => {
          this.loadModels();
        },
        error: (err) => {
          this.error = 'Model silinirken hata oluştu: ' + err.message;
        }
      });
    }
  }

  viewModelDetails(id: number): void {
    this.router.navigate(['/models/view', id]);
  }

  testModel(id: number): void {
    this.router.navigate(['/validation-test', id]);
  }
}