import {
  Component,
  computed,
  inject,
  OnDestroy,
  OnInit,
  ViewChild,
} from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Member } from '../../_models/member';
import { TabDirective, TabsetComponent, TabsModule } from 'ngx-bootstrap/tabs';
import { GalleryItem, GalleryModule, ImageItem } from 'ng-gallery';
import { TimeagoModule } from 'ngx-timeago';
import { DatePipe } from '@angular/common';
import { MemberMessagesComponent } from '../member-messages/member-messages.component';
import { MessageService } from '../../_services/message.service';
import { PresenceService } from '../../_services/presence.service';
import { AccountService } from '../../_services/account.service';
import { HubConnection, HubConnectionState } from '@microsoft/signalr';
import { LikesService } from '../../_services/likes.service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-member-detail',
  standalone: true,
  imports: [
    TabsModule,
    GalleryModule,
    TimeagoModule,
    DatePipe,
    MemberMessagesComponent,
  ],
  templateUrl: './member-detail.component.html',
  styleUrl: './member-detail.component.css',
})
export class MemberDetailComponent implements OnInit, OnDestroy {
  private messageService = inject(MessageService);
  private accountService = inject(AccountService);
  private likesService = inject(LikesService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private toastrService = inject(ToastrService);
  presenceService = inject(PresenceService);
  member: Member = {} as Member;
  images: GalleryItem[] = [];
  @ViewChild('memberTabs', { static: true }) memberTabs?: TabsetComponent;
  activeTab?: TabDirective;
  hasLiked = computed(() =>
    this.likesService.likeIds().includes(this.member.id)
  );

  ngOnInit(): void {
    this.route.data.subscribe({
      next: (data) => {
        this.member = data['member'];
        this.member &&
          this.member.photos.map((p) => {
            this.images.push(new ImageItem({ src: p.url, thumb: p.url }));
          });
      },
    });

    this.route.paramMap.subscribe({
      next: (_) => this.onRouteParamsChange(),
    });

    this.route.queryParams.subscribe({
      next: (params) => {
        params['tab'] && this.selectTab(params['tab']);
      },
    });
  }

  ngOnDestroy(): void {
    this.messageService.stopHubConnection();
  }

  onTabActivated(data: TabDirective) {
    this.activeTab = data;
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { tab: this.activeTab.heading },
      queryParamsHandling: 'merge',
    });

    if (this.activeTab.heading === 'Messages' && this.member) {
      const user = this.accountService.currentUser();
      if (!user) {
        return;
      }

      this.messageService.createHubConnection(user, this.member.userName);
    } else {
      this.messageService.stopHubConnection();
    }
  }

  selectTab(heading: string) {
    if (this.memberTabs) {
      const messageTab = this.memberTabs.tabs.find(
        (tab) => tab.heading === heading
      );

      if (messageTab) {
        messageTab.active = true;
      }
    }
  }

  onRouteParamsChange() {
    const user = this.accountService.currentUser();
    if (!user) {
      return;
    }

    if (
      this.messageService.hubConnection?.state ===
        HubConnectionState.Connected &&
      this.activeTab?.heading === 'Messages'
    ) {
      this.messageService.hubConnection.stop().then(() => {
        this.messageService.createHubConnection(user, this.member.userName);
      });
    }
  }

  toggleLike() {
    this.likesService.toggleLike(this.member.id).subscribe({
      next: () => {
        if (this.hasLiked()) {
          this.likesService.likeIds.update((ids) =>
            ids.filter((p) => p !== this.member.id)
          );
        } else {
          this.likesService.likeIds.update((ids) => [...ids, this.member.id]);
        }
      },
    });
  }
}
