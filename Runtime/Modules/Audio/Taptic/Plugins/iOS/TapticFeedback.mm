#import "AudioToolBox/AudioServices.h"

void Taptic(int type){
    switch (type) {
        case 1:{
            UINotificationFeedbackGenerator *generator =  [[UINotificationFeedbackGenerator alloc] init];
            [generator notificationOccurred:UINotificationFeedbackTypeWarning];
            break;
        }
        case 2:{
            UINotificationFeedbackGenerator *generator =  [[UINotificationFeedbackGenerator alloc] init];
            [generator notificationOccurred:UINotificationFeedbackTypeError];
            break;
        }
        case 3:{
            UINotificationFeedbackGenerator *generator =  [[UINotificationFeedbackGenerator alloc] init];
            [generator notificationOccurred:UINotificationFeedbackTypeSuccess];
            break;
        }
        case 4:{
            UIImpactFeedbackGenerator *generator =  [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleLight];
            [generator impactOccurred];
            [generator prepare];
            break;
        }
        case 5:{
            UIImpactFeedbackGenerator *generator =  [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleMedium];
            [generator impactOccurred];
            [generator prepare];
            break;
        }
        case 6:{
            UIImpactFeedbackGenerator *generator =  [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleHeavy];
            [generator impactOccurred];
            [generator prepare];
            break;
        }
        case 8:{
            UISelectionFeedbackGenerator *generator = [[UISelectionFeedbackGenerator alloc] init];
            [generator selectionChanged];
            break;
        }
        default:
            //Do nothing, should never reach here
            break;
    }
}

void Taptic6s(int type){
    switch (type) {
        case 1:{
            AudioServicesPlaySystemSound(1521);
            break;
        }
        case 2:{
            AudioServicesPlaySystemSound(1521);
            break;
        }
        case 3:{
            AudioServicesPlaySystemSound(1521);
            break;
        }
        case 4:{
            AudioServicesPlaySystemSound(1519);
            break;
        }
        case 5:{
            AudioServicesPlaySystemSound(1519);
            break;
        }
        case 6:{
            AudioServicesPlaySystemSound(1520);
            break;
        }
        case 8:{
            AudioServicesPlaySystemSound(1519);
            break;
        }
        default:
            //Do nothing, should never reach here
            break;
    }
}

extern "C"{
    void ME_PlayTaptic(int type){
        Taptic(type);
    }

    void ME_PlayTaptic6s(int type){
        Taptic6s(type);
    }

}

