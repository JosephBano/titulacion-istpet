import {
  ChangeDetectionStrategy,
  Component,
  Injectable,
  computed,
  input,
  model,
} from '@angular/core';
import { ControlContainer, FormsModule, NgForm } from '@angular/forms';
import {
  DateAdapter,
  MAT_DATE_FORMATS,
  MAT_DATE_LOCALE,
  NativeDateAdapter,
} from '@angular/material/core';
import { MatDatepickerIntl, MatDatepickerModule } from '@angular/material/datepicker';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';

/** Acepta las fechas de la API y la escritura local sin interpretar dd/MM como MM/dd. */
@Injectable()
export class SpanishDateAdapter extends NativeDateAdapter {
  override parse(value: unknown): Date | null {
    if (value instanceof Date) return value;
    if (typeof value !== 'string' || !value.trim()) return null;
    const iso = /^(\d{4})-(\d{2})-(\d{2})$/.exec(value.trim());
    const local = /^(\d{1,2})\/(\d{1,2})\/(\d{4})$/.exec(value.trim());
    if (!iso && !local) return new Date(NaN);
    const year = Number(iso ? iso[1] : local?.[3]);
    const month = Number(iso ? iso[2] : local?.[2]);
    const day = Number(iso ? iso[3] : local?.[1]);
    const date = new Date(year, month - 1, day);
    return year >= 1000 &&
      date.getFullYear() === year &&
      date.getMonth() === month - 1 &&
      date.getDate() === day
      ? date
      : new Date(NaN);
  }
}

function calendarLabels(): MatDatepickerIntl {
  const labels = new MatDatepickerIntl();
  labels.calendarLabel = 'Calendario';
  labels.openCalendarLabel = 'Abrir calendario';
  labels.prevMonthLabel = 'Mes anterior';
  labels.nextMonthLabel = 'Mes siguiente';
  labels.prevYearLabel = 'Año anterior';
  labels.nextYearLabel = 'Año siguiente';
  labels.switchToMonthViewLabel = 'Elegir día';
  labels.switchToMultiYearViewLabel = 'Elegir mes y año';
  return labels;
}

@Component({
  selector: 'app-date-field',
  standalone: true,
  imports: [FormsModule, MatFormFieldModule, MatInputModule, MatDatepickerModule],
  viewProviders: [{ provide: ControlContainer, useExisting: NgForm }],
  providers: [
    { provide: MAT_DATE_LOCALE, useValue: 'es-EC' },
    { provide: DateAdapter, useClass: SpanishDateAdapter },
    { provide: MatDatepickerIntl, useFactory: calendarLabels },
    {
      provide: MAT_DATE_FORMATS,
      useValue: {
        parse: { dateInput: 'dd/MM/yyyy' },
        display: {
          dateInput: { year: 'numeric', month: '2-digit', day: '2-digit' },
          monthYearLabel: { year: 'numeric', month: 'long' },
          dateA11yLabel: { year: 'numeric', month: 'long', day: 'numeric' },
          monthYearA11yLabel: { year: 'numeric', month: 'long' },
        },
      },
    },
  ],
  template: `
    <mat-form-field appearance="outline" subscriptSizing="dynamic">
      <input
        matInput
        [id]="inputId()"
        [name]="name()"
        [ngModel]="dateValue()"
        (ngModelChange)="changeDate($event)"
        [matDatepicker]="calendar"
        [min]="minimumDate()"
        [required]="required()"
        [disabled]="disabled()"
        placeholder="dd/mm/aaaa"
        #field="ngModel"
      />
      <mat-datepicker-toggle matIconSuffix [for]="calendar" />
      <mat-datepicker #calendar panelClass="complexivo-calendar" />
      @if (field.hasError('matDatepickerParse')) {
        <mat-error>Escribe una fecha válida: dd/mm/aaaa.</mat-error>
      } @else if (field.hasError('matDatepickerMin')) {
        <mat-error>La fecha final debe ser igual o posterior a la de inicio.</mat-error>
      } @else if (field.hasError('required')) {
        <mat-error>Selecciona una fecha.</mat-error>
      }
    </mat-form-field>
  `,
  styles: `
    :host {
      display: block;
      min-width: 0;
    }
    mat-form-field {
      width: 100%;
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DateFieldComponent {
  readonly value = model<string | null>('');
  readonly inputId = input.required<string>();
  readonly name = input.required<string>();
  readonly required = input(false);
  readonly disabled = input(false);
  readonly min = input<string | null>(null);
  readonly dateValue = computed(() => this.asDate(this.value()));
  readonly minimumDate = computed(() => this.asDate(this.min()));

  asDate(value: string | null): Date | null {
    return value ? new Date(`${value}T00:00:00`) : null;
  }

  changeDate(date: Date | null): void {
    this.value.set(
      date
        ? `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}-${String(date.getDate()).padStart(2, '0')}`
        : '',
    );
  }
}
