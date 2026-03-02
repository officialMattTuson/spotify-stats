import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'pluck'
})
export class PluckPipe implements PipeTransform {
  transform<T>(array: T[], key: keyof T, separator: string = ', '): string {
    if (!array || !Array.isArray(array)) {
      return '';
    }
    return array.map(item => String(item[key])).join(separator);
  }
}
