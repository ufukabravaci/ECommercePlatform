import { Component, input, computed, ChangeDetectionStrategy } from '@angular/core';

@Component({
  selector: 'app-loading-spinner',
  standalone: true,
  templateUrl: './loading-spinner.html',
  styleUrl: './loading-spinner.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class LoadingSpinnerComponent {
  readonly message = input<string>('Yükleniyor...');
  readonly size = input<'sm' | 'md' | 'lg'>('md');
  readonly overlay = input<boolean>(false);
  readonly minHeight = input<string>('200px');

  readonly spinnerClass = computed(() => {
    const sizeMap = { sm: 'spinner-border-sm', md: '', lg: 'spinner-lg' };
    return `spinner-border text-primary ${sizeMap[this.size()]}`;
  });
}