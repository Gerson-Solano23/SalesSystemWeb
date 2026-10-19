import {
  Component,
  OnDestroy,
  OnInit,
  ChangeDetectionStrategy,
} from '@angular/core';
import { Chart, registerables } from 'chart.js';
import moment from 'moment';
import { PredictionService } from '../../../../Services/prediction.service';
import { SalesForecast } from '../../../../Interfaces/sales-forecast';

Chart.register(...registerables);

@Component({
  selector: 'app-data-prediction',
  templateUrl: './data-prediction.component.html',
  styleUrls: ['./data-prediction.component.css', '../../../../../styles.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false,
})
export class DataPredictionComponent implements OnInit, OnDestroy {
  readonly monthOptions = [1, 3, 6, 12];
  monthsAhead = 3;
  loading = false;
  forecast: SalesForecast | null = null;
  private chart: Chart | null = null;

  constructor(private predictionService: PredictionService) {}

  ngOnInit(): void {
    this.loadForecast();
  }

  ngOnDestroy(): void {
    this.chart?.destroy();
  }

  loadForecast(): void {
    this.loading = true;
    this.predictionService.monthlyRevenue(this.monthsAhead).subscribe({
      next: (response) => {
        this.loading = false;
        this.forecast = response.status
          ? (response.data as SalesForecast)
          : null;
        // Wait for the canvas to be rendered by *ngIf before drawing.
        setTimeout(() => this.drawChart());
      },
      error: () => {
        this.loading = false;
        this.forecast = null;
      },
    });
  }

  get nextForecast() {
    return this.forecast?.forecast?.[0] ?? null;
  }

  monthLabel(month: string): string {
    return moment(month, 'YYYY-MM').format('MMM YYYY');
  }

  private drawChart(): void {
    this.chart?.destroy();
    this.chart = null;
    const data = this.forecast;
    if (
      !data ||
      !data.hasEnoughData ||
      !document.getElementById('forecastChart')
    ) {
      return;
    }

    const history = data.history;
    const labels = [...history, ...data.forecast].map((p) =>
      this.monthLabel(p.month),
    );
    const empty = (count: number) => Array(count).fill(null);
    // The forecast line starts at the last real month so both lines connect.
    const lastActual = history[history.length - 1].revenue;

    this.chart = new Chart('forecastChart', {
      type: 'line',
      data: {
        labels,
        datasets: [
          {
            label: 'Actual revenue',
            data: [
              ...history.map((p) => p.revenue),
              ...empty(data.forecast.length),
            ],
            borderColor: 'rgba(130, 181, 214, 1)',
            backgroundColor: 'rgba(130, 181, 214, 1)',
            tension: 0.2,
          },
          {
            label: 'Forecast',
            data: [
              ...empty(history.length - 1),
              lastActual,
              ...data.forecast.map((p) => p.revenue),
            ],
            borderColor: 'rgba(255, 159, 64, 1)',
            backgroundColor: 'rgba(255, 159, 64, 1)',
            borderDash: [6, 4],
            tension: 0.2,
          },
          {
            label: 'Upper (95%)',
            data: [
              ...empty(history.length),
              ...data.forecast.map((p) => p.upper ?? null),
            ],
            borderColor: 'rgba(255, 159, 64, 0.3)',
            backgroundColor: 'rgba(255, 159, 64, 0.15)',
            pointRadius: 0,
            fill: '+1',
          },
          {
            label: 'Lower (95%)',
            data: [
              ...empty(history.length),
              ...data.forecast.map((p) => p.lower ?? null),
            ],
            borderColor: 'rgba(255, 159, 64, 0.3)',
            pointRadius: 0,
            fill: false,
          },
        ],
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        interaction: { intersect: false, mode: 'index' },
        scales: {
          x: { grid: { color: 'rgba(255, 255, 255, 0.1)' } },
          y: { grid: { color: 'rgba(255, 255, 255, 0.1)' }, min: 0 },
        },
      },
    });
  }
}
